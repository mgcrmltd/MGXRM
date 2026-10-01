using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.ServiceModel;
using System.Text;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

namespace MGXRM.Plugins
{
    public class Plugin : IPlugin
    {
        public class LocalPluginContext
        {
            internal IServiceProvider ServiceProvider
            {
                get;

                private set;
            }

            internal IOrganizationService OrganizationService
            {
                get;

                private set;
            }

            internal IPluginExecutionContext PluginExecutionContext
            {
                get;

                private set;
            }

            internal ITracingService TracingService
            {
                get;

                private set;
            }
            internal StringBuilder _traceString = new StringBuilder();
            public StringBuilder TraceString
            {
                get { return _traceString; }
                set { _traceString = value; }
            }

            public EntityHandler Handler { get; private set; }
            public StringBuilder ExecutionLogs { get; set; }
            private LocalPluginContext()
            {
            }

            internal LocalPluginContext(IServiceProvider serviceProvider)
            {
                if (serviceProvider == null)
                {
                    throw new ArgumentNullException("serviceProvider");
                }

                // Obtain the execution context service from the service provider.
                this.PluginExecutionContext = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));

                // Obtain the tracing service from the service provider.
                this.TracingService = (ITracingService)serviceProvider.GetService(typeof(ITracingService));

                // Obtain the Organization Service factory service from the service provider
                IOrganizationServiceFactory factory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));

                // Use the factory to generate the Organization Service.
                this.OrganizationService = factory.CreateOrganizationService(this.PluginExecutionContext.UserId);
                Handler = new EntityHandler(this);
            }

            internal void Trace(string message)
            {
                if (string.IsNullOrWhiteSpace(message) || this.TracingService == null)
                {
                    return;
                }

                if (this.PluginExecutionContext == null)
                {
                    this.TracingService.Trace(message);
                }
                else
                {
                    this.TracingService.Trace(
                        "{0}, Correlation Id: {1}, Initiating User: {2}",
                        message,
                        this.PluginExecutionContext.CorrelationId,
                        this.PluginExecutionContext.InitiatingUserId);
                }
            }
        }
        public class EntityHandler
        {
            public enum MergeType { PreAndTarget, TargetAndPost }
            public Entity Target { get; private set; }
            public EntityReference TargetReference { get; private set; }
            public EntityReference AssignReference { get; private set; }
            public Entity PreImage { get; private set; }
            public Entity PostImage { get; private set; }
            public String MessageName { get; private set; }
            public EntityReference EntityMoniker { get; private set; }
            public OptionSetValue State { get; private set; }
            public OptionSetValue Status { get; private set; }
            public QueryByAttribute Query { get; private set; } 
            public EntityHandler(LocalPluginContext context)
            {

                MessageName = context.PluginExecutionContext.MessageName;
                context.TracingService.Trace("Mwssage:" + MessageName);
                
                if (MessageName == "Delete" || MessageName == "Associate" || MessageName == "Disassociate" )
                {
                    TargetReference = (EntityReference)context.PluginExecutionContext.InputParameters["Target"];
                }
                else if(MessageName == "Assign")
                {
                    TargetReference = (EntityReference)context.PluginExecutionContext.InputParameters["Target"];
                    AssignReference = (EntityReference)context.PluginExecutionContext.InputParameters["Assignee"];
                }
                else if (context.PluginExecutionContext.InputParameters.ContainsKey("Target") && context.PluginExecutionContext.MessageName != "Retrieve")
                {
                    //-----
                    context.TracingService.Trace("Getting Target - Not Retrieve");
                    context.TracingService.Trace(context.PluginExecutionContext.InputParameters["Target"].ToString());
                    //------
                    Target = (Entity)context.PluginExecutionContext.InputParameters["Target"];
                }
                //Noticed this doesn;t work when testing so commented out. Didn;t need it in the end
                //else if (context.PluginExecutionContext.InputParameters.ContainsKey("Target") && context.PluginExecutionContext.MessageName == "Retrieve")
                //{
                //    //-----
                //    context.TracingService.Trace("Getting Target - Retrieve");
                //    context.TracingService.Trace(context.PluginExecutionContext.InputParameters["Target"].ToString());
                //    //---

                //    Target = (Entity)context.PluginExecutionContext.OutputParameters["BusinessEntity"];
                //}
                if (MessageName == "SetStateDynamicEntity")
                {
                    //-----
                    context.TracingService.Trace("Getting State and Status");
                    context.TracingService.Trace(context.PluginExecutionContext.InputParameters["State"].ToString());
                    context.TracingService.Trace(context.PluginExecutionContext.InputParameters["Status"].ToString());
                    //---
                    EntityMoniker = (EntityReference)context.PluginExecutionContext.InputParameters["EntityMoniker"];
                    State = (OptionSetValue)context.PluginExecutionContext.InputParameters["State"];
                    Status = (OptionSetValue)context.PluginExecutionContext.InputParameters["Status"];
                }
                PreImage = context.PluginExecutionContext.PreEntityImages.ContainsKey("PreImage") ? context.PluginExecutionContext.PreEntityImages["PreImage"] : null;
                PostImage = context.PluginExecutionContext.PostEntityImages.ContainsKey("PostImage") ? context.PluginExecutionContext.PostEntityImages["PostImage"] : null;
                //------
                context.TracingService.Trace("Finished Handler Constructor");
            }


        }
        private Collection<Tuple<int, string, string, Action<LocalPluginContext>>> registeredEvents;

        /// <summary>
        /// Gets the List of events that the plug-in should fire for. Each List
        /// Item is a <see cref="System.Tuple"/> containing the Pipeline Stage, Message and (optionally) the Primary Entity. 
        /// In addition, the fourth parameter provide the delegate to invoke on a matching registration.
        /// </summary>
        protected Collection<Tuple<int, string, string, Action<LocalPluginContext>>> RegisteredEvents
        {
            get
            {
                if (this.registeredEvents == null)
                {
                    this.registeredEvents = new Collection<Tuple<int, string, string, Action<LocalPluginContext>>>();
                }

                return this.registeredEvents;
            }
        }

        /// <summary>
        /// Gets or sets the name of the child class.
        /// </summary>
        /// <value>The name of the child class.</value>
        protected string ChildClassName
        {
            get;

            private set;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Plugin"/> class.
        /// </summary>
        /// <param name="childClassName">The <see cref=" cred="Type"/> of the derived class.</param>
        internal Plugin(Type childClassName)
        {
            this.ChildClassName = childClassName.ToString();
        }

        /// <summary>
        /// Executes the plug-in.
        /// </summary>
        /// <param name="serviceProvider">The service provider.</param>
        /// <remarks>
        /// For improved performance, Microsoft Dynamics CRM caches plug-in instances. 
        /// The plug-in's Execute method should be written to be stateless as the constructor 
        /// is not called for every invocation of the plug-in. Also, multiple system threads 
        /// could execute the plug-in at the same time. All per invocation state information 
        /// is stored in the context. This means that you should not use global variables in plug-ins.
        /// </remarks>
        public void Execute(IServiceProvider serviceProvider)
        {
            if (serviceProvider == null)
            {
                throw new ArgumentNullException("serviceProvider");
            }

            // Construct the Local plug-in context.
            LocalPluginContext localcontext = new LocalPluginContext(serviceProvider);
            string entityName = "";
            string actionName = "";
            string fullPlugingName = "";
            localcontext.Trace(string.Format(CultureInfo.InvariantCulture, "Entered {0}.Execute()", this.ChildClassName));

            try
            {
                //-----
                localcontext.Trace("Try Stage:" + localcontext.PluginExecutionContext.Stage + " Message:" + localcontext.PluginExecutionContext.MessageName);
                //-----
                // Iterate over all of the expected registered events to ensure that the plugin
                // has been invoked by an expected event
                // For any given plug-in event at an instance in time, we would expect at most 1 result to match.
                Action<LocalPluginContext> entityAction =
                    (from a in this.RegisteredEvents
                     where (
                     a.Item1 == localcontext.PluginExecutionContext.Stage &&
                     a.Item2 == localcontext.PluginExecutionContext.MessageName &&
                     (string.IsNullOrWhiteSpace(a.Item3) ? true : a.Item3 == localcontext.PluginExecutionContext.PrimaryEntityName)
                     )
                     select a.Item4).FirstOrDefault();
                //-----
                localcontext.Trace("Got Entity Action ->" + (entityAction == null).ToString());
                //-----
                entityName = localcontext.PluginExecutionContext.PrimaryEntityName;
                if (entityAction != null)
                {
                    //-----
                    localcontext.Trace(" Entity Action Not Null");
                    //-----
                    fullPlugingName = entityAction.Method.DeclaringType.FullName;
                }
                //-----
                localcontext.Trace(entityName + " | " + fullPlugingName);
                //-----
                if (!String.IsNullOrEmpty(fullPlugingName))
                {
                    actionName = fullPlugingName.Split('.')[fullPlugingName.Split('.').Length - 1];
                }
                //-----
                localcontext.Trace(entityName + " | " + fullPlugingName + " | " + actionName+" - "+ localcontext.PluginExecutionContext.MessageName);
                //-----
                if (entityAction != null)
                {
                    localcontext.Trace(string.Format(
                        CultureInfo.InvariantCulture,
                        "{0} is firing for Entity: {1}, Message: {2}",
                        this.ChildClassName,
                        localcontext.PluginExecutionContext.PrimaryEntityName,
                        localcontext.PluginExecutionContext.MessageName));

                    entityAction.Invoke(localcontext);

                    // now exit - if the derived plug-in has incorrectly registered overlapping event registrations,
                    // guard against multiple executions.
                    return;
                }
            }
            catch (FaultException<OrganizationServiceFault> e)
            {
                localcontext.Trace(string.Format(CultureInfo.InvariantCulture, "Exception: {0}", e.ToString()));

                // Handle the exception.
                throw;
            }
            finally
            {
               
            }
        }
    }
}