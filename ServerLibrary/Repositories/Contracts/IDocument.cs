using BaseLibrary.Entities;
using BaseLibrary.Responses;

namespace ServerLibrary.Repositories.Contracts
{
    public interface IDocument
    {
        Task<ResponseList<Document>> GetDocuments(int type);
        Task<ResponseRequest<Document>> GetDocument(int type);
        Task<ResponseRequest<Document>> GetSyllabus(int id);
    }
}
