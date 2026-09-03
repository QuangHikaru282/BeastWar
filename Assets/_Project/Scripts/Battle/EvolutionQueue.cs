using System.Collections.Generic;

/// <summary>
/// Hàng đợi tĩnh chứa các yêu cầu tiến hóa sau khi thú thăng cấp hoặc sau trận đấu.
/// </summary>
public static class EvolutionQueue
{
    public struct EvolutionRequest
    {
        public RuntimeBeastData beast;
        public BeastData targetForm;
    }

    private static readonly Queue<EvolutionRequest> _queue = new Queue<EvolutionRequest>();

    /// <summary>Đưa yêu cầu tiến hóa vào hàng đợi.</summary>
    public static void Enqueue(RuntimeBeastData beast, BeastData targetForm)
    {
        if (beast == null || targetForm == null) return;

        // Tránh đưa trùng yêu cầu cho cùng một thú
        foreach (var req in _queue)
        {
            if (req.beast == beast) return;
        }

        _queue.Enqueue(new EvolutionRequest { beast = beast, targetForm = targetForm });
    }

    /// <summary>Lấy yêu cầu tiếp theo ra khỏi hàng đợi. Trả về false nếu rỗng.</summary>
    public static bool TryDequeue(out EvolutionRequest request)
    {
        if (_queue.Count > 0)
        {
            request = _queue.Dequeue();
            return true;
        }
        request = default;
        return false;
    }

    /// <summary>Còn yêu cầu tiến hóa nào chưa xử lý không?</summary>
    public static bool HasPending => _queue.Count > 0;

    /// <summary>Xóa toàn bộ hàng đợi.</summary>
    public static void Clear() => _queue.Clear();
}
