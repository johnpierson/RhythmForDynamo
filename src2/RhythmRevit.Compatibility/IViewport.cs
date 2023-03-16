namespace RhythmRevit_Compatibility
{
    public interface IViewport
    {
        int[] CompatibleVersions { get; }

        void SetViewTitleLineLength(object viewport, double length);
        void SetViewTitleLocation(object viewport, object location);
        double GetViewTitleLineLength(object viewport);
        object GetViewTitleLocation(object viewport);

    }
}
