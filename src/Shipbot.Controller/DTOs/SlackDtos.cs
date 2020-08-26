using System;
using Newtonsoft.Json;

namespace Shipbot.Controller.DTOs
{
    public partial class ActionPayload
    {
        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("team")]
        public Team Team { get; set; }

        [JsonProperty("user")]
        public User User { get; set; }

        [JsonProperty("api_app_id")]
        public string ApiAppId { get; set; }

        [JsonProperty("token")]
        public string Token { get; set; }

        [JsonProperty("container")]
        public Container Container { get; set; }

        [JsonProperty("trigger_id")]
        public string TriggerId { get; set; }

        [JsonProperty("channel")]
        public Channel Channel { get; set; }

        [JsonProperty("message")]
        public Message Message { get; set; }

        [JsonProperty("response_url")]
        public Uri ResponseUrl { get; set; }

        [JsonProperty("actions")]
        public Action[] Actions { get; set; }
    }

    public partial class Action
    {
        [JsonProperty("action_id")]
        public string ActionId { get; set; }

        [JsonProperty("block_id")]
        public string BlockId { get; set; }

        [JsonProperty("text")]
        public ActionText Text { get; set; }

        [JsonProperty("value")]
        public string Value { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("action_ts")]
        public string ActionTs { get; set; }
    }

    public partial class ActionText
    {
        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("text")]
        public string Text { get; set; }

        [JsonProperty("emoji")]
        public bool Emoji { get; set; }
    }

    public partial class Channel
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }
    }

    public partial class Container
    {
        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("message_ts")]
        public string MessageTs { get; set; }

        [JsonProperty("channel_id")]
        public string ChannelId { get; set; }

        [JsonProperty("is_ephemeral")]
        public bool IsEphemeral { get; set; }
    }

    public partial class Message
    {
        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("subtype")]
        public string Subtype { get; set; }

        [JsonProperty("text")]
        public string Text { get; set; }

        [JsonProperty("ts")]
        public string Ts { get; set; }

        [JsonProperty("username")]
        public string Username { get; set; }

        [JsonProperty("bot_id")]
        public string BotId { get; set; }

        [JsonProperty("blocks")]
        public Block[] Blocks { get; set; }
    }

    // TODO: replace this with IBlock from SlackAPI
    public partial class Block
    {
        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("block_id")]
        public string BlockId { get; set; }

        [JsonProperty("text", NullValueHandling = NullValueHandling.Ignore)]
        public TextElement Text { get; set; }

        [JsonProperty("fields", NullValueHandling = NullValueHandling.Ignore)]
        public TextElement[] Fields { get; set; }

        [JsonProperty("elements", NullValueHandling = NullValueHandling.Ignore)]
        public Element[] Elements { get; set; }
    }

    public partial class Element
    {
        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("action_id")]
        public string ActionId { get; set; }

        [JsonProperty("text")]
        public ActionText Text { get; set; }

        [JsonProperty("value")]
        public string Value { get; set; }
    }

    public partial class TextElement
    {
        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("text")]
        public string Text { get; set; }

        [JsonProperty("verbatim")]
        public bool Verbatim { get; set; }
    }

    public partial class Team
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("domain")]
        public string Domain { get; set; }
    }

    public partial class User
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("username")]
        public string Username { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("team_id")]
        public string TeamId { get; set; }
    }
}