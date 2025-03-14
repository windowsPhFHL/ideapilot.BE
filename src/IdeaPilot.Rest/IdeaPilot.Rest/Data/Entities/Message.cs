﻿namespace IdeaPilot.Rest.Data.Entities;

public sealed class Message: BaseDocument
{
    //add id as a property, initialize it with a new guid and append `Message_` as a prefix
    public string id { get; set; } = Guid.NewGuid().ToString("N").Insert(0, "Message_");

    public string UserId { get; set; }

    public string ChatId { get; set; }

    public string WorkspaceId { get; set; }

    public string Text { get; set; }

    public string Status { get; set; } = "sent";

    //add a property for the containerType and initialize it with `Message`
    public string ContainerType { get; set; } = "Message";
}