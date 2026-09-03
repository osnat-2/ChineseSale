using Microsoft.AspNetCore.Identity.UI.Services;
using Project.BLL.Interfaces;
using Project.DAL.Interfaces;
using Project.Models;
using Project.Validators;
using System.Net.Mail;

namespace Project.BLL
{
    public class EmailService : IEmailService
    {
        private readonly IEmailSender _emailSender;
        private readonly IWinnerDal _winnerDal;

        public EmailService(IEmailSender emailSender, IWinnerDal winnerDal)
        {
            _emailSender = emailSender;
            _winnerDal = winnerDal;
        }

        //public async Task<Result<string>> SendWinnerEmailAsync(Models.User winner, Models.Present present)
        //{
        //    if (winner == null || present == null)
        //    {
        //        return new Result<string>
        //        {
        //            Success = false,
        //            Message = "Invalid winner or present"
        //        };
        //    }

        //    if (!Validator.ValidEmail(winner.Email))
        //    {
        //        return new Result<string>
        //        {
        //            Success = false,
        //            Message = "Invalid email address"
        //        };
        //    }

        //    try
        //    {
        //        var subject = "Congratulations! You won a prize!";
        //        var htmlBody = ComposeWinnerEmail(winner, present);

        //        await _emailSender.SendEmailAsync("39215866328@mby.co.il", subject, htmlBody);

        //        return new Result<string>
        //        {
        //            Success = true,
        //            Message = "Email sent successfully"
        //        };
        //    }
        //    catch (Exception ex)
        //    {
        //        return new Result<string>
        //        {
        //            Success = false,
        //            Message = $"Error sending email: {ex.Message}"
        //        };
        //    }
        //}

        //public async Task<Result<string>> SendAllWinnersEmailAsync()
        //{
        //    var presentsResult = await _winnerDAL.GetPresentsWithUsersAsync();

        //    if (!presentsResult.Success || presentsResult.Data == null || !presentsResult.Data.Any())
        //    {
        //        return new Result<string>
        //        {
        //            Success = false,
        //            Message = "No winners to send email"
        //        };
        //    }

        //    int sent = 0;
        //    foreach (var kvp in presentsResult.Data.ElementAt(0))
        //    {
        //        var present = kvp.Key;
        //        var users = kvp.Value;
                
        //        if (present == null || users == null || !users.Any())
        //            continue;

        //        foreach (var user in users)
        //        {
        //            var res = await SendWinnerEmailAsync(user, present);
        //            if (res != null && res.Success)
        //                sent++;
        //        }
        //    }

        //    return new Result<string>
        //    {
        //        Success = true,
        //        Message = $"Emails processed: {sent}"
        //    };
        //}

        ///// <summary>
        ///// Compose the HTML email body for winner notification
        ///// </summary>
        //private string ComposeWinnerEmail(Models.User winner, Models.Present present)
        //{
        //    return $@"
        //        <html>
        //            <head>
        //                <style>
        //                    body {{
        //                        font-family: Arial, sans-serif;
        //                        background-color: #f4f4f4;
        //                        color: #333;
        //                        margin: 0;
        //                        padding: 0;
        //                    }}
        //                    .email-container {{
        //                        background-color: #ffffff;
        //                        max-width: 600px;
        //                        margin: 20px auto;
        //                        padding: 20px;
        //                        border-radius: 8px;
        //                        box-shadow: 0 4px 8px rgba(0, 0, 0, 0.1);
        //                    }}
        //                    .header {{
        //                        background-color: #4CAF50;
        //                        color: white;
        //                        padding: 10px;
        //                        text-align: center;
        //                        border-radius: 8px 8px 0 0;
        //                    }}
        //                    .content {{
        //                        margin: 20px 0;
        //                        font-size: 16px;
        //                    }}
        //                    .footer {{
        //                        text-align: center;
        //                        font-size: 14px;
        //                        color: #888;
        //                    }}
        //                    .button {{
        //                        display: inline-block;
        //                        background-color: #4CAF50;
        //                        color: white;
        //                        padding: 10px 20px;
        //                        text-decoration: none;
        //                        border-radius: 5px;
        //                        margin-top: 20px;
        //                    }}
        //                </style>
        //            </head>
        //            <body>
        //                <div class='email-container'>
        //                    <div class='header'>
        //                        <h1>🎉 Congratulations {winner.Name}!</h1>
        //                    </div>
        //                    <div class='content'>
        //                        <p>Dear {winner.Name},</p>
        //                        <p>We are excited to announce that you have won the prize for the gift: <strong>{present.Name}</strong>.</p>
        //                        <p>The prize value is: <strong>{present.Price:C}</strong>.</p>
        //                        <p>We will contact you in the coming days regarding how to receive your prize.</p>
        //                        <p>Here are the details of your prize:</p>
        //                        <ul>
        //                            <li><strong>Gift:</strong> {present.Name}</li>
        //                            <li><strong>Prize Value:</strong> {present.Price:C}</li>
        //                        </ul>
        //                        <p>Thank you for participating, and we hope you enjoy your prize!</p>
        //                        <a href='#' class='button'>View Your Prize</a>
        //                    </div>
        //                    <div class='footer'>
        //                        <p>Best regards,<br>Your Company</p>
        //                        <p><a href='http://www.yourcompany.com' style='color: #4CAF50;'>Visit our website</a></p>
        //                    </div>
        //                </div>
        //            </body>
        //        </html>
        //    ";
        //}
    }
}