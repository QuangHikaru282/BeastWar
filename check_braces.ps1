$text = Get-Content 'e:\Unity\Projects\BeastBall\Assets\_Project\Scripts\Battle\BattleManager.cs' -Raw;
$count = 0;
$line = 1;
foreach ($char in $text.ToCharArray()) {
    if ($char -eq '{') { $count++ }
    elseif ($char -eq '}') { $count-- }
    elseif ($char -eq "`n") { $line++ }
    if ($count -lt 0) {
        Write-Output "Too many } at line $line"
        break
    }
}
Write-Output "Final count: $count"
