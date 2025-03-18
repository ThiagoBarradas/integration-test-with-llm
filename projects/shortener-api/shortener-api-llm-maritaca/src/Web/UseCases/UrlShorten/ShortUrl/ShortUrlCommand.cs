﻿using MediatR;
using Web.Common.Models.Endpoints;

namespace Web.UseCases.UrlShorten.ShortUrl;

public class ShortUrlCommand : IRequest<Result<ShortUrlResponse>>
{
    public string? Url { get; set; }
    public int? ExpireMinutes { get; set; }
    public bool? HasQrCode { get; set; }
}