using BaseLibrary.Entities;
using BaseLibrary.Responses;
using Microsoft.EntityFrameworkCore;
using ServerLibrary.Data;
using ServerLibrary.Repositories.Contracts;

namespace ServerLibrary.Repositories.Implementations
{
    public class DocumentRepository(AppDbContext appDbContext) : IDocument
    {
        public async Task<ResponseRequest<Document>> GetDocument(int type)
        {
            bool flag = false;
            Document documentResult = new Document();
            try
            {
                documentResult = await appDbContext.Documents.Where(d => d.Type == type).FirstOrDefaultAsync();
                flag = true;
            }
            catch (Exception ex)
            {

            }
            return new ResponseRequest<Document>(flag, documentResult);
        }

        public async Task<ResponseList<Document>> GetDocuments(int type)
        {
            bool flag = false;
            List<Document> documentsResult = new List<Document>();
            try
            {
                documentsResult = await appDbContext.Documents.Where(d => d.Type == type).ToListAsync();
                flag = true;
            }
            catch (Exception ex)
            {

            }
            return new ResponseList<Document>(flag, documentsResult);
        }

        public async Task<ResponseRequest<Document>> GetSyllabus(int id)
        {
            bool flag = false;
            Document documentResult = new Document();
            try
            {
                documentResult = await appDbContext.Documents.Where(d => d.DocumentId == id).FirstOrDefaultAsync();
                flag = true;
            }
            catch (Exception ex)
            {
            }
            return new ResponseRequest<Document>(flag, documentResult);
        }
    }
}
