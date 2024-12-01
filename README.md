# EmptyParking
Main idea is to find a free slots to park in

### How to start?

1. Clone repo
2. Start containers
3. Configure pgadmin4_container
   Go tu URL: http://localhost:5050/
   login with - login: admin@admin.com, password: root
   Right click on Servers. Register -> Server
   Tab "General" -> Name: Any name (ex. "My server")
   Tab "Connection" -> Host name/address: 192.168.0.22
                       Port: 5432
                       Maintenance database: master
                       User: postgis
                       Password: 1111
4. Run script on MASTER database -> "Create_parkspots_table_with_data.txt" (plik is at repo)

For testing
1. Install K6 -> run script from file at powerShell"K6 configuration for powerShell.txt"(plik is at repo)
For running test file
1. Open cmd and run script
      .\k6.exe run "<Full test file location>" (ex. .\k6.exe run "C:\EmptyParking\Test.js")