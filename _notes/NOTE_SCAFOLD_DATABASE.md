### Preparation
```
dotnet add package Microsoft.EntityFrameworkCore.Design
dotnet add package Npgsql.EntityFrameworkCore.PostgreSQL
dotnet add package Microsoft.EntityFrameworkCore.SqlServer
dotnet add package Pomelo.EntityFrameworkCore.MySql
dotnet add package Microsoft.EntityFrameworkCore.Sqlite
```

### Generate dbcontext utk webservice mobile JKN (Pure SQL Server)
```
dotnet ef dbcontext scaffold "Server=127.0.0.1;Database=tobasa_openjkn;Uid=openjkn;Pwd=MRinjani3726;" Microsoft.EntityFrameworkCore.SqlServer -o Entities.MsSql --context-dir Data -c DataContextAntrean --namespace Tobasa.Entities --no-pluralize --force -t base_roles -t base_sites -t base_user_role -t base_user_site -t base_users -t bl_mrn_gen -t bl_pasien -t bl_jadwal_bedah -t bl_poli -t bl_antrian -t bl_dokter -t bl_jadwal -t mjkn_poli_sub -t mjkn_poli -t mjkn_dokter -t mjkn_pasien -t mjkn_ambil_antrean -t mjkn_jadwal_dokter -t mjkn_antrean_transaction
```

### Generate dbcontext utk webservice mobile JKN (Pure MySQL Server)
```
dotnet ef dbcontext scaffold "Server=localhost;User=openjkn;Password=MRinjani3726;Database=tobasa_openjkn" "Pomelo.EntityFrameworkCore.MySql"  -o Entities.Mysql --context-dir Data -c DataContextAntreanMySql --namespace Tobasa.Entities --no-pluralize --force -t base_roles -t base_sites -t base_user_role -t base_user_site -t base_users -t bl_mrn_gen -t bl_pasien -t bl_jadwal_bedah -t bl_poli -t bl_antrian -t bl_dokter -t bl_jadwal -t mjkn_poli_sub -t mjkn_poli -t mjkn_dokter -t mjkn_pasien -t mjkn_ambil_antrean -t mjkn_jadwal_dokter -t mjkn_antrean_transaction
```

### Generate dbcontext utk webservice mobile JKN (Pure PostgreSQL)
```
dotnet ef dbcontext scaffold "Server=127.0.0.1;Database=tobasa_openjkn;Username=openjkn;Password=MRinjani3726;Port=5432" Npgsql.EntityFrameworkCore.PostgreSQL -o Entities.Pgsql --context-dir Data -c DataContextAntrianPostgreSql --namespace Tobasa.Entities --no-pluralize --force -t base_roles -t base_sites -t base_user_role -t base_user_site -t base_users -t bl_id_gen -t bl_pasien -t bl_jadwal_bedah -t bl_poli -t bl_antrian -t bl_dokter -t bl_jadwal -t mjkn_poli_sub -t mjkn_poli -t mjkn_dokter -t mjkn_pasien -t mjkn_ambil_antrian -t mjkn_jadwal_dokter -t mjkn_antrian_transaction
```

### Generate dbcontext utk webservice mobile JKN (Pure SQLite)
```
dotnet ef dbcontext scaffold "Data Source=tobasa_openjkn.db3" Microsoft.EntityFrameworkCore.Sqlite  -o Entities.Sqlite --context-dir Data -c DataContextAntreanSqlite --namespace Tobasa.Entities --no-pluralize --force -t base_roles -t base_sites -t base_user_role -t base_user_site -t base_users -t bl_mrn_gen -t bl_pasien -t bl_jadwal_bedah -t bl_poli -t bl_antrian -t bl_dokter -t bl_jadwal -t mjkn_poli_sub -t mjkn_poli -t mjkn_dokter -t mjkn_pasien -t mjkn_ambil_antrean -t mjkn_jadwal_dokter -t mjkn_antrean_transaction
```