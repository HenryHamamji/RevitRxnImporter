using Autodesk.Revit.UI;

namespace RevitReactionImporter
{
    abstract class BaseIdleHandler : IExternalEventHandler
    {
        public bool EventRegistered { get; set; }
        protected UIApplication app;

        public void Execute(UIApplication app)
        {
            if (!EventRegistered)
            {
                EventRegistered = true;
                app.Idling += Application_DocumentIdling;
            }
        }

        void Application_DocumentIdling(object sender,
            Autodesk.Revit.UI.Events.IdlingEventArgs e)
        {
            app = sender as UIApplication;
            EventRegistered = false;
            app.Idling -= Application_DocumentIdling;

            Run();
        }

        public abstract void Run();

        public abstract string GetName();
    }
}
