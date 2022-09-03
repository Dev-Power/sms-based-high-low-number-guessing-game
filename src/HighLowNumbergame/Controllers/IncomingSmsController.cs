using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Twilio.AspNet.Core;
using Twilio.TwiML;

namespace HighLowNumbergame.Controllers;
 
[ApiController]
[Route("[controller]")]
public class IncomingSmsController : TwilioController
{
    Game currentGame;
    bool CHEAT_MODE = true;
    
    [HttpPost]
    public async Task<TwiMLResult> Index()
    {
        var form = await Request.ReadFormAsync();

        currentGame = ResumeOrCreateGame();
        
        var userMessage = form["Body"].ToString().Trim().ToLowerInvariant();
        var responseMessage = string.Empty;
        
        if (userMessage == "play")
        {
            currentGame = new Game();
            responseMessage = $"Welcome to number guessing game. Send your guesses between {Constants.MIN_NUMBER} and {Constants.MAX_NUMBER}";
            Response.Cookies.Append("GAME_DATA", JsonConvert.SerializeObject(currentGame));
        }
        else if (userMessage == "exit")
        {
            if (currentGame == null)
            {
                responseMessage = "No game in progress";
            }
            else
            {
                responseMessage = $"Quiting game. The target was {currentGame.Target}. You guesses {currentGame.GuessCount} times. Better luck next time!";
                Response.Cookies.Delete("GAME_DATA");                
            }
        }
        else if (int.TryParse(userMessage, out int guessedNumber))
        {
            if (currentGame == null)
            {
                responseMessage = "No game in progress";
            }
            else
            {
                if (guessedNumber < Constants.MIN_NUMBER || guessedNumber > Constants.MAX_NUMBER)
                {
                    responseMessage = $"Please guess between {Constants.MIN_NUMBER} and {Constants.MAX_NUMBER}";
                }
                else if (guessedNumber == currentGame.Target)
                {
                    currentGame.GuessCount++;
                    responseMessage = $"Congratulations!. You've guessed correctly in {currentGame.GuessCount} guesses.";
                    Response.Cookies.Delete("GAME_DATA");
                }
                else
                {
                    currentGame.GuessCount++;
                    
                    if (guessedNumber > currentGame.Target)
                    {
                        responseMessage = "Too high!";
                    }
                    else if (guessedNumber < currentGame.Target)
                    {
                        responseMessage = "Too low!";
                    }

                    Response.Cookies.Append("GAME_DATA", JsonConvert.SerializeObject(currentGame));
                }
            }
        }
        else
        {
            responseMessage = "Unknown command";
        }
        
        var messagingResponse = new MessagingResponse();

        if (CHEAT_MODE && currentGame != null)
        {
            responseMessage = $"{responseMessage}\n{JsonConvert.SerializeObject(currentGame)}";
        }

        messagingResponse.Message(responseMessage);
        return TwiML(messagingResponse);
    }

    private Game ResumeOrCreateGame()
    {
        var cookies = Request.Cookies;
        if (cookies.TryGetValue("GAME_DATA", out string rawGameJson))
        {
            return JsonConvert.DeserializeObject<Game>(rawGameJson);
        }

        return new Game();
    }
}