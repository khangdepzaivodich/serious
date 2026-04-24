$json = Get-Content -Raw -Encoding UTF8 'f:\blazor\Services\CatalogService\Data\data.json' | ConvertFrom-Json
foreach($ldm in $json) {
    foreach($dm in $ldm.DanhMucs) {
        foreach($sp in $dm.SanPhams) {
            if ($null -eq $sp.LuotBan) {
                Add-Member -InputObject $sp -MemberType NoteProperty -Name 'LuotBan' -Value 0
            }
        }
    }
}
$json | ConvertTo-Json -Depth 10 | Set-Content -Path 'f:\blazor\Services\CatalogService\Data\data.json' -Encoding UTF8
