$text = Get-Content 'e:\Unity\Projects\BeastBall\Assets\_Project\Scripts\Battle\BattleManager.cs'
$count = 0
for ($i = 0; $i -lt $text.Length; $i++) {
    $line = $text[$i]
    if ($line -match "private void OnPlayerActionChosen" -or $line -match "private IEnumerator EnemyTurn" -or $line -match "private bool CheckBattleEnd" -or $line -match "public void HandleBeastClick" -or $line -match "private void ReturnToMap" -or $line -match "private IEnumerator EndBattle") {
        Write-Output "Line $($i+1): $line [Level: $count]"
    }
    foreach ($char in $line.ToCharArray()) {
        if ($char -eq '{') { $count++ }
        elseif ($char -eq '}') { $count-- }
    }
}
Write-Output "Final count: $count"
