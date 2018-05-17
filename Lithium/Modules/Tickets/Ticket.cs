﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Lithium.Discord.Contexts;
using Lithium.Discord.Contexts.Paginator;
using Lithium.Models;

namespace Lithium.Modules.Tickets
{
    [Group("Ticket")]
    public class Ticket : Base
    {
        [Command]
        [Summary("Ticket")]
        [Remarks("List all Tickets")]
        public async Task Tickets()
        {
            if (!Context.Server.Tickets.tickets.Any())
            {
                await ReplyAsync("There are no tickets in this server");
                return;
            }
            var pages = new List<PaginatedMessage.Page>();
            foreach (var ticket in Context.Server.Tickets.tickets.OrderBy(x => x.id))
            {
                pages.Add(new PaginatedMessage.Page
                {
                    description = $"Ticket By: {Context.Socket.Guild.GetUser(ticket.InitUser)?.Username ?? $"Missing User [{ticket.InitUser}]"}\n" +
                    $"Message: {ticket.message}\n\n" +
                    $"^ [{ticket.Up.Count}] v [{ticket.Down.Count}]\n" +
                    $"ID: {ticket.id}\n" +
                    $"Solved: {ticket.solved}"
                });
            }
            var pager = new PaginatedMessage
            {
                Title = "Server Tickets",
                Pages = pages
            };
            await PagedReplyAsync(pager);
        }
        [Command("Solved")]
        [Summary("Ticket Solved")]
        [Remarks("List all Solved Tickets")]
        public async Task STickets()
        {
            if (!Context.Server.Tickets.tickets.Any())
            {
                await ReplyAsync("There are no tickets in this server");
                return;
            }
            var pages = new List<PaginatedMessage.Page>();
            foreach (var ticket in Context.Server.Tickets.tickets.OrderBy(x => x.id).Where(x => x.solved))
            {
                pages.Add(new PaginatedMessage.Page
                {
                    description = $"Ticket By: {Context.Socket.Guild.GetUser(ticket.InitUser)?.Username ?? $"Missing User [{ticket.InitUser}]"}\n" +
                                  $"Message: {ticket.message}\n\n" +
                                  $"^ [{ticket.Up.Count}] v [{ticket.Down.Count}]\n" +
                                  $"ID: {ticket.id}\n" +
                                  $"Solved Messsage:\n" +
                                  $"{ticket.solvedmessage ?? "N/A"}"
                });
            }
            var pager = new PaginatedMessage
            {
                Title = "Server Tickets",
                Pages = pages
            };
            await PagedReplyAsync(pager);
        }
        [Command("UnSolved")]
        [Summary("Ticket UnSolved")]
        [Remarks("List all UnSolved Tickets")]
        public async Task USTickets()
        {
            if (!Context.Server.Tickets.tickets.Any())
            {
                await ReplyAsync("There are no tickets in this server");
                return;
            }
            var pages = new List<PaginatedMessage.Page>();
            foreach (var ticket in Context.Server.Tickets.tickets.OrderBy(x => x.id).Where(x => !x.solved))
            {
                pages.Add(new PaginatedMessage.Page
                {
                    description = $"Ticket By: {Context.Socket.Guild.GetUser(ticket.InitUser)?.Username ?? $"Missing User [{ticket.InitUser}]"}\n" +
                                  $"Message: {ticket.message}\n\n" +
                                  $"^ [{ticket.Up.Count}] v [{ticket.Down.Count}]\n" +
                                  $"ID: {ticket.id}\n" +
                                  $"Comment Count: {ticket.comments.Count}"
                });
            }
            var pager = new PaginatedMessage
            {
                Title = "Server Tickets",
                Pages = pages
            };
            await PagedReplyAsync(pager);
        }

        [Command("Create")]
        [Summary("Ticket Create <ticket message>")]
        [Remarks("Create a ticket")]
        public async Task Create([Remainder]string message = null)
        {
            if (message == null)
            {
                await ReplyAsync("To create a ticket, simply use this command and provide a message for server moderations or admins to solve");
                return;
            }

            var ticket = new GuildModel.Guild.ticketing.ticket
            {
                message = message,
                comments = new List<GuildModel.Guild.ticketing.ticket.comment>(),
                id = Context.Server.Tickets.tickets.Count,
                solved = false,
                InitUser = Context.User.Id
            };
            Context.Server.Tickets.tickets.Add(ticket);
            Context.Server.Save();
            var ticketemb = new EmbedBuilder
            {
                Title = $"New Ticket by {Context.User.Username}",
                Description = $"Message:\n" +
                              $"{ticket.message}\n\n" +
                              $"Creator: {Context.User.Mention} `[{Context.User.Id}]`\n" +
                              $"ID: `{ticket.id}`\n" +
                              $"Users may Upvote this by using the `Vote Up <ID>` command or downvote using the `Vote Down <ID>` command"
            };

            await SendEmbedAsync(ticketemb);
        }

        [Command("Vote Up")]
        [Summary("Ticket Vote Up <ID>")]
        [Remarks("Upvote a ticket")]
        public async Task Up(int id = -1)
        {
            if (id == -1)
            {
                await ReplyAsync("Please select a ticket to upvote. You can see a list of public tickets using the `TicketList` Command");
                return;
            }
            var targetticket = Context.Server.Tickets.tickets.FirstOrDefault(x => x.id == id);
            if (targetticket == null)
            {
                await ReplyAsync("There is no ticket with that ID.");
                return;
            }

            if (targetticket.solved)
            {
                await ReplyAsync("You cannot vote on completed tickets!");
                return;
            }

            if (targetticket.Down.Contains(Context.User.Id))
            {
                targetticket.Down.Remove(Context.User.Id);
            }
            var ticketemb = new EmbedBuilder
            {
                Color = Color.Orange,
                Footer = new EmbedFooterBuilder
                {
                    Text = $"{Context.User.Username} Voted"
                }
            };
            if (targetticket.Up.Contains(Context.User.Id))
            {
                targetticket.Up.Remove(Context.User.Id);
                ticketemb.Title = "Removed Upvote";
            }
            else
            {
                targetticket.Up.Add(Context.User.Id);
                ticketemb.Title = "Upvoted Ticket";
            }

            ticketemb.Description = $"Ticket By: {Context.Socket.Guild.GetUser(targetticket.InitUser)?.Username ?? $"Missing User [{targetticket.InitUser}]"}\n" +
                                    $"Message: {targetticket.message}\n\n" +
                                    $"^ [{targetticket.Up.Count}] v [{targetticket.Down.Count}]\n" +
                                    $"ID: {targetticket.id}";

            await SendEmbedAsync(ticketemb);
            Context.Server.Save();
        }

        [Command("Vote Down")]
        [Summary("Ticket Vote Down <ID>")]
        [Remarks("Downvote a ticket")]
        public async Task Down(int id = -1)
        {
            if (id == -1)
            {
                await ReplyAsync("Please select a ticket to Downvote. You can see a list of public tickets using the `TicketList` Command");
                return;
            }
            var targetticket = Context.Server.Tickets.tickets.FirstOrDefault(x => x.id == id);
            if (targetticket == null)
            {
                await ReplyAsync("There is no ticket with that ID.");
                return;
            }

            if (targetticket.solved)
            {
                await ReplyAsync("You cannot vote on completed tickets!");
                return;
            }

            if (targetticket.Up.Contains(Context.User.Id))
            {
                targetticket.Up.Remove(Context.User.Id);
            }
            var ticketemb = new EmbedBuilder
            {
                Color = Color.Blue,
                Footer = new EmbedFooterBuilder
                {
                    Text = $"{Context.User.Username} Voted"
                }
            };
            if (targetticket.Down.Contains(Context.User.Id))
            {
                targetticket.Down.Remove(Context.User.Id);
                ticketemb.Title = "Removed Downvote";
            }
            else
            {
                targetticket.Down.Add(Context.User.Id);
                ticketemb.Title = "Downvoted Ticket";
            }

            ticketemb.Description = $"Ticket By: {Context.Socket.Guild.GetUser(targetticket.InitUser)?.Username ?? $"Missing User [{targetticket.InitUser}]"}\n" +
                                    $"Message: {targetticket.message}\n\n" +
                                    $"^ [{targetticket.Up.Count}] v [{targetticket.Down.Count}]\n" +
                                    $"ID: {targetticket.id}";

            await SendEmbedAsync(ticketemb);
            Context.Server.Save();
        }

        [Command("Comment")]
        [Summary("Ticket Comment <ID> <Comment>")]
        [Remarks("Comment on a ticket")]
        public async Task Comment(int id, [Remainder]string comment = null)
        {
            var targetticket = Context.Server.Tickets.tickets.FirstOrDefault(x => x.id == id);
            if (targetticket == null)
            {
                await ReplyAsync("There is no ticket with that ID.");
                return;
            }

            if (targetticket.solved)
            {
                await ReplyAsync("You cannot comment on completed tickets!");
                return;
            }

            targetticket.comments.Add(new GuildModel.Guild.ticketing.ticket.comment
            {
                Comment = comment,
                id = targetticket.comments.Count,
                UserID = Context.User.Id
            });
            Context.Server.Save();

            var pages = new List<PaginatedMessage.Page>
            {
                new PaginatedMessage.Page
                    {
                        description = $"Ticket By: {Context.Socket.Guild.GetUser(targetticket.InitUser)?.Username ?? $"Missing User [{targetticket.InitUser}]"}\n" +
                                    $"Message: {targetticket.message}\n\n" +
                                    $"^ [{targetticket.Up.Count}] v [{targetticket.Down.Count}]\n" +
                                      $"ID: {targetticket.id}"

                    }
            };

            var desc = "";
            foreach (var c in targetticket.comments.OrderByDescending(x => x.id))
            {
                desc += $"User: {Context.Socket.Guild.GetUser(c.UserID)?.Username ?? $"Unknown User `[{c.UserID}]`"}\n" +
                        $"Comment:\n" +
                        $"{c.Comment}\n" +
                        $"ID: {c.id}\n" +
                        $"^ [{c.Up.Count}] v [{c.Down.Count}]\n\n";
                if (desc.Length > 800)
                {
                    pages.Add(new PaginatedMessage.Page
                    {
                        description = desc
                    });
                    desc = "";
                }
            }
            pages.Add(new PaginatedMessage.Page
            {
                description = desc
            });

            var paginator = new PaginatedMessage
            {
                Title = $"{Context.User.Username} Just Commented!",
                Pages = pages,
                Color = Color.DarkOrange
            };
            await PagedReplyAsync(paginator);
        }

        [Command("Vote Comment Up")]
        [Summary("Ticket Vote Comment Up <TicketID> <CommentID>")]
        [Remarks("Vote on a comment of a ticket")]
        public async Task UpComment(int ticketID, int CommentID)
        {
            var targetticket = Context.Server.Tickets.tickets.FirstOrDefault(x => x.id == ticketID);
            if (targetticket == null)
            {
                await ReplyAsync("There is no ticket with that ID.");
                return;
            }

            var comment = targetticket.comments.FirstOrDefault(x => x.id == CommentID);
            if (comment == null)
            {
                await ReplyAsync($"No Comment with the ID of {CommentID} in Ticket {ticketID}");
                return;
            }
            if (comment.Down.Contains(Context.User.Id))
            {
                comment.Down.Remove(Context.User.Id);
            }

            string upvoteaction;
            if (comment.Up.Contains(Context.User.Id))
            {
                comment.Up.Remove(Context.User.Id);
                upvoteaction = "Removed";
            }
            else
            {
                comment.Up.Add(Context.User.Id);
                upvoteaction = "Added";
            }
            Context.Server.Save();
            var pages = new List<PaginatedMessage.Page>
            {
                new PaginatedMessage.Page
                    {
                        description = $"Ticket By: {Context.Socket.Guild.GetUser(targetticket.InitUser)?.Username ?? $"Missing User [{targetticket.InitUser}]"}\n" +
                                    $"Message: {targetticket.message}\n\n" +
                                    $"^ [{targetticket.Up.Count}] v [{targetticket.Down.Count}]\n" +
                                      $"ID: {targetticket.id}"

                    },
                    new PaginatedMessage.Page
                    {
                        description = $"Comment By: { Context.Socket.Guild.GetUser(comment.UserID)?.Username ?? $"Missing User [{comment.UserID}]"}\n" +
                                    $"Message: {comment.Comment}\n\n" +
                                    $"^ [{comment.Up.Count}] v [{comment.Down.Count}]\n" +
                                    $"ID: {comment.id}"
                    }
            };

            var paginator = new PaginatedMessage
            {
                Title = $"Ticket #{targetticket.id} Comment #{comment.id} Upvote {upvoteaction}",
                Pages = pages,
                Color = Color.DarkOrange
            };
            await PagedReplyAsync(paginator);
        }

        [Command("Vote Comment Down")]
        [Summary("Ticket Vote Comment Down <TicketID> <CommentID>")]
        [Remarks("Downvote on a comment of a ticket")]
        public async Task DownComment(int ticketID, int CommentID)
        {
            var targetticket = Context.Server.Tickets.tickets.FirstOrDefault(x => x.id == ticketID);
            if (targetticket == null)
            {
                await ReplyAsync("There is no ticket with that ID.");
                return;
            }

            var comment = targetticket.comments.FirstOrDefault(x => x.id == CommentID);
            if (comment == null)
            {
                await ReplyAsync($"No Comment with the ID of {CommentID} in Ticket {ticketID}");
                return;
            }
            if (comment.Up.Contains(Context.User.Id))
            {
                comment.Up.Remove(Context.User.Id);
            }

            string downaction;
            if (comment.Down.Contains(Context.User.Id))
            {
                comment.Down.Remove(Context.User.Id);
                downaction = "Removed";
            }
            else
            {
                comment.Down.Add(Context.User.Id);
                downaction = "Added";
            }
            Context.Server.Save();
            var pages = new List<PaginatedMessage.Page>
            {
                new PaginatedMessage.Page
                    {
                        description = $"Ticket By: {Context.Socket.Guild.GetUser(targetticket.InitUser)?.Username ?? $"Missing User [{targetticket.InitUser}]"}\n" +
                                    $"Message: {targetticket.message}\n\n" +
                                    $"^ [{targetticket.Up.Count}] v [{targetticket.Down.Count}]\n" +
                                      $"ID: {targetticket.id}"

                    },
                    new PaginatedMessage.Page
                    {
                        description = $"Comment By: { Context.Socket.Guild.GetUser(comment.UserID)?.Username ?? $"Missing User [{comment.UserID}]"}\n" +
                                    $"Message: {comment.Comment}\n\n" +
                                    $"^ [{comment.Up.Count}] v [{comment.Down.Count}]\n" +
                                    $"ID: {comment.id}"
                    }
            };

            var paginator = new PaginatedMessage
            {
                Title = $"Ticket #{targetticket.id} Comment #{comment.id} Downvote {downaction}",
                Pages = pages,
                Color = Color.DarkOrange
            };
            await PagedReplyAsync(paginator);
        }

        [Command("Comments")]
        [Summary("Ticket Comments <ID>")]
        [Remarks("View comments of a ticket")]
        public async Task Comment(int id)
        {
            var targetticket = Context.Server.Tickets.tickets.FirstOrDefault(x => x.id == id);
            if (targetticket == null)
            {
                await ReplyAsync("There is no ticket with that ID.");
                return;
            }

            var pages = new List<PaginatedMessage.Page>
            {
                new PaginatedMessage.Page
                    {
                        description = $"Ticket By: {Context.Socket.Guild.GetUser(targetticket.InitUser)?.Username ?? $"Missing User [{targetticket.InitUser}]"}\n" +
                                    $"Message: {targetticket.message}\n\n" +
                                    $"^ [{targetticket.Up.Count}] v [{targetticket.Down.Count}]\n" +
                                      $"ID: {targetticket.id}"

                    }
            };

            var desc = "";
            foreach (var c in targetticket.comments.OrderByDescending(x => x.id))
            {
                desc += $"User: {Context.Socket.Guild.GetUser(c.UserID)?.Username ?? $"Unknown User `[{c.UserID}]`"}\n" +
                        $"Comment:\n" +
                        $"{c.Comment}\n" +
                        $"ID: {c.id}\n" +
                        $"^ [{c.Up.Count}] v [{c.Down.Count}]\n\n";
                if (desc.Length > 800)
                {
                    pages.Add(new PaginatedMessage.Page
                    {
                        description = desc
                    });
                    desc = "";
                }
            }
            pages.Add(new PaginatedMessage.Page
            {
                description = desc
            });

            var paginator = new PaginatedMessage
            {
                Title = $"Ticket #{targetticket.id} Comments",
                Pages = pages,
                Color = Color.DarkOrange
            };
            await PagedReplyAsync(paginator);
        }
    }
}
