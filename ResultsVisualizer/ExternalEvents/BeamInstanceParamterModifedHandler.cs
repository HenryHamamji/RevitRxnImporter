using Autodesk.Revit.UI;

namespace RevitReactionImporter
{
    class BeamInstanceParamterModifedHandler : BaseIdleHandler
    {
        public ResultsVisualizer.ParameterUpdater ParamUpdater { get; set; }

        public override void Run()
        {
            if (ParamUpdater == null)
                return;

            ParamUpdater.UpdateVisualizationHistoryWithNewUserDefinedParam();
            ParamUpdater.UpdateUserDefinedVisualization();
        }

        public override string GetName()
        {
            return "ImportRAMBeamSizingHandler";
        }
    }
}


