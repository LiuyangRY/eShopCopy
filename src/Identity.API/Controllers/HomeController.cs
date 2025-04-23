using Identity.API.Models.Account;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Identity.API.Controllers;

/// <summary>
/// 主页控制器
/// </summary>
[AllowAnonymous]
public class HomeController : Controller
{
    private readonly IWebHostEnvironment _hostingEnvironment;
    private readonly ILogger _logger;

    /// <summary>
    /// 主页控制器构造函数
    /// </summary>
    /// <param name="logger">日志</param>
    /// <param name="hostingEnvironment">宿主环境</param>
    public HomeController(ILogger<HomeController> logger, IWebHostEnvironment hostingEnvironment)
    {
        _logger = logger;
        _hostingEnvironment = hostingEnvironment;
    }

    /// <summary>
    /// 索引页
    /// </summary>
    public IActionResult Index()
    {
        if (_hostingEnvironment.IsDevelopment())
        {
            return View();
        }
        _logger.LogInformation("生产环境中禁止使用主页，返回404");
        return NotFound();
    }

    /// <summary>
    /// 错误页
    /// </summary>
    /// <param name="errorId">错误id</param>
    /// <returns>错误页</returns>
    public async Task<IActionResult> Error(string errorId)
    {
        ErrorViewModel? viewModel = null;
        // var message = await _interaction.GetErrorContextAsync(errorId);
        // if (message != null)
        // {
        //     viewModel = new ErrorViewModel(message);
        //     if (!_hostingEnvironment.IsDevelopment())
        //     {
        //         message.ErrorDescription = null;
        //     }
        // }
        await Task.CompletedTask;
        return View("Error", viewModel);
    }
}
