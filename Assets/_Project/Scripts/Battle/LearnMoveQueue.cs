using System.Collections.Generic;

/// <summary>
/// Queue tĩnh chứa các yêu cầu học chiêu mới được phát sinh trong trận đấu.
/// 
/// Luồng:
///   LevelUpManager phát hiện chiêu mới tại level X
///     → Enqueue(beast, newMove)
///   Sau khi trận kết thúc, BattleManager drain queue
///     → Gọi LearnMoveUI để hỏi người chơi
/// 
/// Dùng static queue để truyền dữ liệu giữa LevelUpManager và BattleManager
/// mà không cần reference trực tiếp lẫn nhau.
/// </summary>
public static class LearnMoveQueue
{
    public struct LearnRequest
    {
        public RuntimeBeastData beast;
        public MoveData newMove;
    }

    private static readonly Queue<LearnRequest> _queue = new Queue<LearnRequest>();

    /// <summary>Đưa yêu cầu học chiêu vào hàng đợi.</summary>
    public static void Enqueue(RuntimeBeastData beast, MoveData move)
    {
        _queue.Enqueue(new LearnRequest { beast = beast, newMove = move });
    }

    /// <summary>Lấy yêu cầu tiếp theo ra khỏi hàng đợi. Trả về false nếu rỗng.</summary>
    public static bool TryDequeue(out LearnRequest request)
    {
        if (_queue.Count > 0)
        {
            request = _queue.Dequeue();
            return true;
        }
        request = default;
        return false;
    }

    /// <summary>Còn yêu cầu chưa xử lý không?</summary>
    public static bool HasPending => _queue.Count > 0;

    /// <summary>Xóa toàn bộ queue — dùng khi bắt đầu trận mới.</summary>
    public static void Clear() => _queue.Clear();
}
