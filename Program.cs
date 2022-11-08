namespace TrailersTest
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddRazorPages();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();

            }

            // Including this causes FireFox and/or IIS on Windows Server 2022 to not download the full response content.
            // Running this locally in Visual Studio, served by Kestrel on Windows does not exhibit the problem.
            app.Use(async (context, next) =>
            {
                if (AllowsTrailers(context.Request) && context.Response.SupportsTrailers())
                {
                    // Declare that there will be a trailer after the response body.
                    context.Response.DeclareTrailer("Server-Timing");

                    // Continue
                    await next(context);

                    // Output trailer, just some dummy values
                    context.Response.AppendTrailer(
                        "Server-Timing",
                        "total;dur=123.4");
                }
                else
                {
                    await next(context);
                }

                /// <summary>
                /// Most browsers say they support trailers, but it's a byproduct of http/2 and they
                /// actually discard any trailers present.  FireFox, as of May 2021, is the lone exception.  
                /// FF will send the "TE" header with a value of "trailers".
                /// </summary>
                static bool AllowsTrailers(HttpRequest request)
                {
                    return request.Headers.ContainsKey("TE") &&
                        request.Headers["TE"].Contains("trailers");
                }
            });

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthorization();

            app.MapRazorPages();

            app.Run();
        }
    }
}