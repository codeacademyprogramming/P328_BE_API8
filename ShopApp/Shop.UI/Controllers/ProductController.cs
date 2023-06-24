using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Net.Http.Headers;
using Newtonsoft.Json;
using Shop.UI.Filters;
using Shop.UI.Models;
using System.Collections.Generic;
using System.Net.Http.Headers;

namespace Shop.UI.Controllers
{
    [ServiceFilter(typeof(AuthFilter))]
    public class ProductController : Controller
    {
        HttpClient _client;
        public ProductController()
        {
            _client = new HttpClient();
        }

        public async Task<IActionResult> Index()
        {
            var token = Request.Cookies["auth_token"];
            _client.DefaultRequestHeaders.Add(HeaderNames.Authorization, token);

            using (var response = await _client.GetAsync("https://localhost:7171/api/products/all"))
            {
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var data = JsonConvert.DeserializeObject<List<ProductGetAllItemResponse>>(content);

                    return View(data);
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized || response.StatusCode == System.Net.HttpStatusCode.Forbidden)
                    return RedirectToAction("login", "account");
            }
            return View("error");
        }

        public async Task<IActionResult> Create()
        {
            var token = Request.Cookies["auth_token"];
            _client.DefaultRequestHeaders.Add(HeaderNames.Authorization, token);

            ViewBag.Brands = await _getBrands();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ProductCreateRequest product)
        {
            if (!ModelState.IsValid)
            {
                using (var response = await _client.GetAsync("https://localhost:7171/api/Brands/all"))
                {
                    if (response.IsSuccessStatusCode)
                    {
                        var brandContent = await response.Content.ReadAsStringAsync();
                        var data = JsonConvert.DeserializeObject<List<BrandGetAllItemResponse>>(brandContent);

                        ViewBag.Brands = data;
                        return View();
                    }
                    else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized || response.StatusCode == System.Net.HttpStatusCode.Forbidden)
                        return RedirectToAction("login", "account");
                }
            }

            var token = Request.Cookies["auth_token"];
            _client.DefaultRequestHeaders.Add(HeaderNames.Authorization, token);

            MultipartFormDataContent content = new MultipartFormDataContent();

            var fileContent = new StreamContent(product.Image.OpenReadStream());
            fileContent.Headers.ContentType = System.Net.Http.Headers.MediaTypeHeaderValue.Parse(product.Image.ContentType);

            content.Add(fileContent, "ImageFile", product.Image.FileName);
            content.Add(new StringContent(product.Name), "Name");
            content.Add(new StringContent(product.CostPrice.ToString()), "CostPrice");
            content.Add(new StringContent(product.DiscountPercent.ToString()), "DiscountPercent");
            content.Add(new StringContent(product.SalePrice.ToString()), "SalePrice");
            content.Add(new StringContent(product.BrandId.ToString()), "BrandId");


            using (var respone = await _client.PostAsync("https://localhost:7171/api/Products", content))
            {
                if (respone.IsSuccessStatusCode)
                {
                    return RedirectToAction("index");
                }
                else if (respone.StatusCode == System.Net.HttpStatusCode.BadRequest)
                {
                    var error = JsonConvert.DeserializeObject<ErrorResponseModel>(await respone.Content.ReadAsStringAsync());
                    foreach (var err in error.Errors) ModelState.AddModelError(err.Key, err.Message);

                    ViewBag.Brands = await _getBrands();

                    return View();
                }
            }

            return View("error");
        }


        public async Task<IActionResult> Edit(int id)
        {
            var token = Request.Cookies["auth_token"];
            _client.DefaultRequestHeaders.Add(HeaderNames.Authorization, token);

            using(var response = await _client.GetAsync($"https://localhost:7171/api/products/{id}"))
            {
                if(response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    ProductGetResponse data = JsonConvert.DeserializeObject<ProductGetResponse>(responseContent);

                    var vm = new ProductUpdateRequest
                    {
                        BrandId = data.Brand.Id,
                        Name = data.Name,
                        DiscountPercent = data.DiscountPercent,
                        SalePrice = data.SalePrice,
                        CostPrice = data.CostPrice,
                    };

                    ViewBag.ImgUrl = data.ImageUrl;
                    ViewBag.Brands = await _getBrands();
                    return View(vm);
                }
            }
            return View("error");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id,ProductUpdateRequest product)
        {
            if (!ModelState.IsValid)
            {
               ViewBag.Brands = await _getBrands();
                return View();
            }

            var token = Request.Cookies["auth_token"];
            _client.DefaultRequestHeaders.Add(HeaderNames.Authorization, token);

            MultipartFormDataContent content = new MultipartFormDataContent();

            if (product.Image != null)
            {
                var fileContent = new StreamContent(product.Image.OpenReadStream());
                fileContent.Headers.ContentType = System.Net.Http.Headers.MediaTypeHeaderValue.Parse(product.Image.ContentType);
                content.Add(fileContent, "ImageFile", product.Image.FileName);
            }
           
            content.Add(new StringContent(product.Name), "Name");
            content.Add(new StringContent(product.CostPrice.ToString()), "CostPrice");
            content.Add(new StringContent(product.DiscountPercent.ToString()), "DiscountPercent");
            content.Add(new StringContent(product.SalePrice.ToString()), "SalePrice");
            content.Add(new StringContent(product.BrandId.ToString()), "BrandId");


            using (var respone = await _client.PutAsync($"https://localhost:7171/api/Products/{id}", content))
            {
                if (respone.IsSuccessStatusCode)
                {
                    return RedirectToAction("index");
                }
                else if (respone.StatusCode == System.Net.HttpStatusCode.BadRequest)
                {
                    var error = JsonConvert.DeserializeObject<ErrorResponseModel>(await respone.Content.ReadAsStringAsync());
                    foreach (var err in error.Errors) ModelState.AddModelError(err.Key, err.Message);

                    ViewBag.Brands = await _getBrands();

                    return View();
                }
            }

            return View("error");
        }
        private async Task<List<BrandGetAllItemResponse>> _getBrands()
        {
            List<BrandGetAllItemResponse> brands = new List<BrandGetAllItemResponse>();
            using (var response = await _client.GetAsync("https://localhost:7171/api/Brands/all"))
            {
                if (response.IsSuccessStatusCode)
                {
                    var brandContent = await response.Content.ReadAsStringAsync();
                    brands = JsonConvert.DeserializeObject<List<BrandGetAllItemResponse>>(brandContent);
                }
            }

            return brands;
        }
    }
}
