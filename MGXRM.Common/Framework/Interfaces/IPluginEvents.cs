namespace MGXRM.Common.Framework.Interfaces
{
    public interface IPluginEvents
    {
        void PreCreate();
        void PostCreate();
        void PostCreateSync();
        void PostCreateAsync();

        void PreSetState();
        void PostSetState();
        void PostSetStateSync();
        void PostSetStateAsync();

        void PreSetStateDynamicEntity();
        void PostSetStateDynamicEntity();
        void PostSetStateDynamicEntitySync();
        void PostSetStateDynamicEntityAsync();

        void PreUpdate();
        void PostUpdate();
        void PostUpdateSync();
        void PostUpdateAsync();

        void PostAssign();
        void PreAssign();
        void PostAssignSync();
        void PostAssignAsync();

        void PreClose();
        void PostClose();
        void PostCloseSync();
        void PostCloseAsync();
    }
}