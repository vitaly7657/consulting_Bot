using consulting_telegram_bot.Models;
using consulting_telegram_bot.ViewModels;
using Newtonsoft.Json;
using System.Text;

namespace consulting_telegram_bot
{
    public class ConsultingDataApi
    {
        private HttpClient httpClient { get; set; }
        public ConsultingDataApi()
        {
            httpClient = new HttpClient();
        }

        //создать заявку
        public async Task<string> CreateRequest(string request_name, string request_email, string request_text)
        {
            if (request_name == "" || request_email == "" || request_text == "")
            {
                return "Заполните все поля";
            }
            string url = @"https://localhost:44380/api/application/request/";
            RequestViewModel req = new RequestViewModel
            {
                RequesterName = request_name,
                RequestEmail = request_email,
                RequestText = request_text
            };
            var result = await httpClient.PostAsync(
                requestUri: url,
                content: new StringContent(JsonConvert.SerializeObject(req), Encoding.UTF8,
                mediaType: "application/json")
                );
            if (result.StatusCode != System.Net.HttpStatusCode.OK)
            {
                return "Ошибка обработки запроса";
            }

            return "ok";
        }

        //ТЕКСТЫ------------------------------------------------------------
        //запрос реквизитов
        public MainClass GetRequisitesTexts()
        {
            //все тексты страницы (в том числе реквизиты)
            string urlText = @"https://localhost:44380/api/application/sitetext/";
            string jsonText = httpClient.GetStringAsync(urlText).Result;

            MainClass mc = new MainClass();
            mc.siteText = JsonConvert.DeserializeObject<SiteText>(jsonText);

            return mc;
        }

        //УСЛУГИ------------------------------------------------------------
        //запрос перечня услуг
        public async Task<IEnumerable<Service>> GetAllServices()
        {
            string urlServices = @"https://localhost:44380/api/application/services/";
            string jsonServices = await httpClient.GetStringAsync(urlServices);
            var servicesList = JsonConvert.DeserializeObject<IEnumerable<Service>>(jsonServices);
            return servicesList;
        }

        //ПРОЕКТЫ------------------------------------------------------------
        //запрос перечня проектов
        public async Task<IEnumerable<Project>> GetAllProjects()
        {
            string urlProjects = @"https://localhost:44380/api/application/projects/";
            string jsonProjects = await httpClient.GetStringAsync(urlProjects);
            var projectsList = JsonConvert.DeserializeObject<IEnumerable<Project>>(jsonProjects);
            return projectsList;
        }

        //запрос проекта по Id
        public async Task<Project> GetProjectById(int id)
        {
            string urlProject = $@"https://localhost:44380/api/application/projects/{id}";
            string jsonProject = await httpClient.GetStringAsync(urlProject);
            var project = JsonConvert.DeserializeObject<Project>(jsonProject);
            return project;
        }

        //БЛОГ------------------------------------------------------------
        //запрос перечня блогов
        public async Task<IEnumerable<Blog>> GetAllBlogs()
        {
            string urlBlogs = @"https://localhost:44380/api/application/blog/";
            string jsonBlogs = await httpClient.GetStringAsync(urlBlogs);            
            var blogList = JsonConvert.DeserializeObject<IEnumerable<Blog>>(jsonBlogs);
            return blogList;
        }

        //запрос блога по Id
        public async Task<Blog> GetBlogById(int id)
        {
            string urlBlog = $@"https://localhost:44380/api/application/blog/{id}";
            string jsonBlog = await httpClient.GetStringAsync(urlBlog);
            var blog = JsonConvert.DeserializeObject<Blog>(jsonBlog);
            return blog;
        }

        //КОНТАКТЫ------------------------------------------------------------
        //запрос контактов
        public async Task<List<Models.Contact>> GetAllContacts()
        {
            string urlContacts = @"https://localhost:44380/api/application/contacts/";
            string jsonContacts = await httpClient.GetStringAsync(urlContacts);
            var contactsList = JsonConvert.DeserializeObject<List<Models.Contact>>(jsonContacts);
            return contactsList;
        }
    }
}
