$text = Get-Content 'e:\Unity\Projects\BeastBall\Assets\_Project\Scripts\Battle\BattleManager.cs'
$count = 0
for ($i = 0; $i -lt $text.Length; $i++) {
    $line = $text[$i]
    if ($line -match "private void Start" -or $line -match "private IEnumerator RunBattle" -or $line -match "private IEnumerator PlayerTurn") {
        Write-Output "Line $($i+1): $line [Level: $count]"
    }
    foreach ($char in $line.ToCharArray()) {
        if ($char -eq '{') { $count++ }
        elseif ($char -eq '}') { $count-- }
    }
}
