using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace eft_dma_radar.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class GameController : ControllerBase
    {
        [HttpGet("players")]
        public IActionResult GetPlayers()
        {
            try
            {
                var players = Memory.Players; // Assuming Memory is a static class managing game data
                if (players == null || !players.Any())
                {
                    return NotFound("No players found.");
                }

                var playerData = players.Values.Select(player => new
                {
                    player.Name,
                    player.IsPMC,
                    player.IsLocalPlayer,
                    player.IsAlive,
                    player.IsActive,
                    player.Lvl,
                    player.KDA,
                    player.ProfileID,
                    player.AccountID,
                    Gear = player.Gear.Select(g => new
                    {
                        Slot = g.Key,
                        LongName = g.Value?.Long, 
                        ShortName = g.Value?.Short, 
                        ItemValue = g.Value?.Value 
                    }),
                    Position = new
                    {
                        X = player.Position.X,
                        Y = player.Position.Y,
                        Z = player.Position.Z
                    },
                    Rotation = new
                    {
                        Yaw = player.Rotation.X,  // Horizontal rotation
                        Pitch = player.Rotation.Y // Vertical rotation
                    }
                });

                return Ok(playerData);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetPlayers: {ex.Message}");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("map")]
        public IActionResult GetMapInfo()
        {
            try
            {
                var mapName = Memory.MapNameFormatted; // Assuming this is the method to get the current map name
                var mapState = new
                {
                    MapName = mapName,
                    // Include other relevant map details here
                };

                return Ok(mapState);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetMapInfo: {ex.Message}");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("state")]
        public IActionResult GetGameState()
        {
            try
            {
                var state = new
                {
                    InGame = Memory.InGame,
                    InHideout = Memory.InHideout,
                    IsScav = Memory.IsScav,
                    MapName = Memory.MapNameFormatted,
                    LoadingLoot = Memory.LoadingLoot
                };

                return Ok(state);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetGameState: {ex.Message}");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("loot/loose")]
        public IActionResult GetLooseLoot()
        {
            try
            {
                if (Memory.Loot == null || Memory.Loot.Loot == null)
                {
                    Console.WriteLine("Loot data is still loading.");
                    return StatusCode(503, "Loot data is still loading. Please try again later.");
                }
        
                var loot = Memory.Loot.Loot.OfType<LootItem>().ToList();
                if (!loot.Any())
                {
                    return NotFound("No loose loot found.");
                }
        
                var lootData = loot.Select(item => new
                {
                    item.Name,
                    item.ID,
                    item.Value,
                    Position = new
                    {
                        X = item.Position.X,
                        Y = item.Position.Y,
                        Z = item.Position.Z
                    }
                });
        
                return Ok(lootData);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetLooseLoot: {ex.Message}");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("loot/containers")]
        public IActionResult GetLootContainers()
        {
            try
            {
                // Ensure the loot manager has been initialized and contains items
                if (Memory.Loot == null || !Memory.Loot.HasCachedItems)
                {
                    return NotFound("Loot data is not available.");
                }

                var lootContainers = Memory.Loot.Loot.OfType<LootContainer>()
                    .Select(container => new
                    {
                        container.Name,
                        container.Items,
                        container.Value,
                        container.Important, 
                        Position = new
                    {
                        X = container.Position.X,
                        Y = container.Position.Y,
                        Z = container.Position.Z
                    }
                    })
                    .ToList();

                return Ok(lootContainers);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetLootContainers: {ex.Message}");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("loot/corpses")]
        public IActionResult GetLootCorpses()
        {
            try
            {
                // Ensure the loot manager has been initialized and contains items
                if (Memory.Loot == null || !Memory.Loot.HasCachedItems)
                {
                    return NotFound("Loot data is not available.");
                }

                var lootCorpses = Memory.Loot.Loot.OfType<LootCorpse>()
                    .Select(corpse => new
                    {
                        corpse.Name,
                        corpse.Items,
                        corpse.Value,
                        corpse.Important,
                    Position = new
                    {
                        X = corpse.Position.X,
                        Y = corpse.Position.Y,
                        Z = corpse.Position.Z
                    }
                    })
                    .ToList();

                return Ok(lootCorpses);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetLootCorpses: {ex.Message}");
                return StatusCode(500, "Internal server error");
            }
        }
        [HttpGet("loot/quests")]
        public IActionResult GetQuestItemsAndZones()
        {
            try
            {
                // Ensure the quest manager is initialized and contains quest items and zones
                if (Memory.QuestManager == null || Memory.QuestManager.QuestItems == null || Memory.QuestManager.QuestZones == null)
                {
                    return NotFound("Quest items or zones data is not available.");
                }
        
                // Process Quest Items
                var questItems = Memory.QuestManager.QuestItems
                    .Where(item => item.Position.X != 0) // Filter out items with default (0,0,0) positions
                    .Select(item => new
                    {
                        item.Id,
                        item.Name,
                        item.ShortName,
                        item.TaskName,
                        item.Description,
                        Position = new
                        {
                            X = item.Position.X,
                            Y = item.Position.Y,
                            Z = item.Position.Z
                        }
                    })
                    .ToList();
        
                // Process Quest Zones
                var questZones = Memory.QuestManager.QuestZones
                    .Where(zone => zone.Position.X != 0) // Filter out zones with default (0,0,0) positions
                    .Select(zone => new
                    {
                        zone.ID,
                        zone.TaskName,
                        zone.Description,
                        zone.ObjectiveType,
                        Position = new
                        {
                            X = zone.Position.X,
                            Y = zone.Position.Y,
                            Z = zone.Position.Z
                        }
                    })
                    .ToList();
        
                return Ok(new { questItems, questZones });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetQuestItemsAndZones: {ex.Message}");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("exfils")]
        public IActionResult GetExfils()
        {
            try
            {
                var exfils = Memory.Exfils; // Assuming Memory.Exfils returns the current exfil data
                if (exfils == null || !exfils.Any())
                {
                    return NotFound("No exfil points found.");
                }

                var exfilData = exfils.Select(exfil => new
                {
                    exfil.Name,
                    exfil.Status,
                    Position = new
                    {
                        X = exfil.Position.X,
                        Y = exfil.Position.Y,
                        Z = exfil.Position.Z
                    }
                });

                return Ok(exfilData);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetExfils: {ex.Message}");
                return StatusCode(500, "Internal server error");
            }
        }


        [HttpGet("/ws/connect")]
        public async Task Connect()
        {
            if (HttpContext.WebSockets.IsWebSocketRequest)
            {
                // Fetch the game state
                var gameState = new
                {
                    InGame = Memory.InGame,
                    InHideout = Memory.InHideout,
                    IsScav = Memory.IsScav
                };

                // Only proceed if the game is in progress
                if (gameState.InGame)
                {
                    Console.WriteLine("Starting WebSocket");
                    using WebSocket webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();
                    await SendUpdates(webSocket);
                }
                else
                {
                    HttpContext.Response.StatusCode = 503; // Service Unavailable
                    Console.WriteLine("Stopping WebSocket" + gameState);
                }
            }
            else
            {
                HttpContext.Response.StatusCode = 400; // Bad Request
            }
        }

        private async Task SendUpdates(WebSocket webSocket)
        {
            while (webSocket.State == WebSocketState.Open)
            {
                var updateData = new
                {
                    players = Memory.Players.Values.Select(player => new
                    {
                        player.Name,
                        player.IsPMC,
                        player.IsLocalPlayer,
                        player.IsAlive,
                        player.IsActive,
                        player.Lvl,
                        player.KDA,
                        player.ProfileID,
                        player.AccountID,
                        player.Type, // Include the Player Type
                        Gear = player.Gear.Select(g => new
                        {
                            Slot = g.Key,
                            LongName = g.Value?.Long,
                            ShortName = g.Value?.Short,
                            ItemValue = g.Value?.Value
                        }),
                        Position = new
                        {
                            X = player.Position.X,
                            Y = player.Position.Y,
                            Z = player.Position.Z
                        },
                        Rotation = new
                        {
                            Yaw = player.Rotation.X,  // Horizontal rotation
                            Pitch = player.Rotation.Y // Vertical rotation
                        }
                    }).ToList(),

                    loot = Memory.Loot.Loot.OfType<LootItem>().Select(l => new
                    {
                        l.Name,
                        l.ID,
                        l.Value,
                        Position = new { l.Position.X, l.Position.Y, l.Position.Z }
                    }).ToList(),

                    exfils = Memory.Exfils.Select(exfil => new
                    {
                        exfil.Name,
                        exfil.Status,
                        Position = new { exfil.Position.X, exfil.Position.Y, exfil.Position.Z }
                    }).ToList(),

                    //corpses = Memory.Loot.Loot.OfType<LootCorpse>().Select(corpse => new
                    //{
                    //    corpse.Name,
                    //    corpse.Items,
                    //    corpse.Value,
                    //    corpse.Important,
                    //    Position = new
                    //    {
                    //        X = corpse.Position.X,
                    //        Y = corpse.Position.Y,
                    //        Z = corpse.Position.Z
                    //    }
                    //}).ToList(),
//
                    //containers = Memory.Loot.Loot.OfType<LootContainer>().Select(container => new
                    //{
                    //    container.Name,
                    //    container.Items,
                    //    container.Value,
                    //    container.Important,
                    //    Position = new
                    //    {
                    //        X = container.Position.X,
                    //        Y = container.Position.Y,
                    //        Z = container.Position.Z
                    //    }
                    //}).ToList()
                };

                var jsonString = JsonSerializer.Serialize(updateData);
                var bytes = Encoding.UTF8.GetBytes(jsonString);
                var buffer = new ArraySegment<byte>(bytes);

                await webSocket.SendAsync(buffer, WebSocketMessageType.Text, true, CancellationToken.None);

                await Task.Delay(100); // Adjust the delay to control the update frequency
            }
        }
    }
}