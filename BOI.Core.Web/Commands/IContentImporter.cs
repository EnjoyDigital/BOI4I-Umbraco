namespace BOI.Core.Web.Commands
{
    public interface IContentImporter
    {
        ImporterResponse ProcessImportFile(ImporterRequest importRequest);
    }
}