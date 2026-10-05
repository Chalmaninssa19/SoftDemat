namespace SoftDemat.Application.DTOs;

public sealed record DispatchResultResponse(
    int SentCount,
    int NotSentCount,
    IReadOnlyList<DispatchItemResponse> Items);
