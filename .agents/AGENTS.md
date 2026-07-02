# Antigravity Rules for BeastWar Project

Khi bạn (Antigravity) hỗ trợ người dùng trong dự án Unity này, BẮT BUỘC tuân thủ các quy tắc làm việc nhóm (Collaboration Rules) sau đây để hướng dẫn họ, nhằm tránh lỗi Merge Conflict trầm trọng với Git:

1. BẢO VỆ SCENE (SCENE PROTECTION):
- Khuyên người dùng KHÔNG sửa trực tiếp các GameObject trên Scene trừ khi họ là người chịu trách nhiệm chính (Owner) của Scene đó (VD: người xây Map).
- Hãy nhắc họ dùng tính năng "Multi-Scene Editing" (LoadSceneMode.Additive) nếu họ cần kết hợp các Scene lại với nhau thay vì copy/paste đồ đạc chung vào 1 file Scene.

2. QUY TẮC PREFAB:
- Mọi thay đổi đối với nhân vật, vật phẩm, quái vật... phải được thực hiện trong PREFAB MODE.
- Luôn hướng dẫn người dùng: "Hãy click đúp vào file `.prefab` ở thư mục Project để mở chế độ nền xanh (Prefab Mode) rồi mới chỉnh sửa thông số".

3. XỬ LÝ MERGE CONFLICT TRONG UNITY:
- Nếu người dùng gặp xung đột (Conflict) ở các file cấu hình Unity như `.unity` (Scene) hoặc `.prefab`, TUYỆT ĐỐI KHÔNG khuyên họ dùng text editor (như VS Code/Notepad) để gộp (merge) text thủ công. Điều này sẽ làm hỏng cấu trúc YAML và hỏng vĩnh viễn file đó.
- Hãy khuyên họ: "Phải chấp nhận dùng trọn vẹn 1 bản và bỏ 1 bản. Hãy trao đổi với đồng đội xem bản của ai đang chứa các tính năng quan trọng hơn, sau đó chọn 'Use modified file from origin' (Dùng bản trên mạng) hoặc 'Use modified file from local' (Dùng bản của bạn)".

4. BẢO VỆ TRƯỚC KHI COMMIT:
- Nhắc nhở người dùng luôn kiểm tra kỹ cửa sổ "Changed files" trước khi bấm nút Commit. Nếu có file `.unity` bị thay đổi mà không phải chủ đích của họ, hãy hướng dẫn họ nhấp chuột phải và chọn "Discard changes" để xoá phần lỡ tay đó đi.

5. HƯỚNG DẪN RÕ VỊ TRÍ CHỈNH SỬA (LOCATION SPECIFICITY):
- Khi hướng dẫn người dùng kéo thả UI, thêm component hoặc chỉnh sửa GameObject trong Unity Editor, LUÔN LUÔN ghi rõ vị trí thao tác. 
- Ví dụ cụ thể: "Hãy mở Prefab Player lên (click đúp để vào chế độ nền xanh) và kéo UI vào", hoặc "Hãy kéo nó làm con của Player thay vì để ngoài Scene".
- Điều này giúp nhắc nhở người dùng không làm lộn xộn Scene chính và bảo toàn quy tắc Prefab.
