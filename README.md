# Progetto di Autenticazione JWT Fullstack 

Implementazione di un sistema di autenticazione basato su JSON Web Token (JWT). L'applicazione è composta da:

* Un **frontend** sviluppato in Angular, che gestisce l'interfaccia utente per la registrazione e il login.
* Un **backend** sviluppato con ASP.NET Core Web API, che si occupa della logica di autenticazione, della gestione degli utenti e della generazione dei token JWT.
* I dati degli utenti vengono salvati in un database SQL Server (gestibile tramite SSMS o un altro client SQL).

## Come Funziona l'Autenticazione JWT Implementata

1.  **Registrazione Utente**:
    * L'utente inserisce username, email e password nel form di registrazione del client Angular.
    * Il frontend invia questi dati all'endpoint `/api/auth/register` del backend.
    * Il backend ASP.NET Core:
        * Verifica se l'username o l'email esistono già.
        * Esegue l'hashing della password con un "salt" univoco (una stringa casuale) per motivi di sicurezza (usando PBKDF2-SHA256). Non memorizza mai la password in chiaro.
        * Salva il nuovo utente (username, email, hash della password, salt) nel database SQL Server.
    * Viene inviato un messaggio di successo al frontend.

2.  **Login Utente**:
    * L'utente inserisce username e password nel form di login.
    * Il frontend invia le credenziali all'endpoint `/api/auth/login` del backend.
    * Il backend ASP.NET Core:
        * Recupera l'utente dal database in base all'username.
        * Se l'utente esiste, esegue l'hashing della password fornita usando il "salt" memorizzato per quell'utente.
        * Confronta l'hash appena generato con quello memorizzato nel database.
        * Se corrispondono, genera un Token JWT (stringa codificata che contiene info sull'utente e una data di scadenza. È firmato digitalmente dal server usando una chiave segreta (configurata nel file `appsettings.json` del backend). Questa firma garantisce che il token sia autentico.)
        * Il backend invia il Token JWT al frontend.
    * Il frontend Angular riceve il token e lo memorizza (ad esempio, nel `localStorage` del browser).

3.  **Accesso a Risorse Protette**:
    * Quando l'utente tenta di accedere a una pagina protetta dell'applicazione Angular:
        * Un "AuthGuard" in Angular verifica se esiste un token valido e non scaduto. Se non c'è, reindirizza l'utente alla pagina di login.
    * Se l'utente vuole recuperare dati da un endpoint protetto del backend (es. `/api/data/me`):
        * Il frontend Angular (idealmente tramite un "HttpInterceptor" non implementato qui) allega automaticamente il Token JWT all'header `Authorization` di ogni richiesta HTTP inviata al backend (nel formato `Bearer TUO_TOKEN`).
        * Il backend ASP.NET Core, prima di consentire l'accesso all'endpoint protetto:
            * Verifica la validità del Token JWT (firma, scadenza, issuer, audience) usando la stessa chiave segreta.
            * Se il token è valido, l'accesso è consentito e i dati vengono restituiti. Altrimenti, viene restituito un errore (es. 401 Unauthorized).

4.  **Logout**:
    * L'utente clicca "Logout".
    * Il frontend Angular rimuove il Token JWT memorizzato.
    * L'utente viene reindirizzato alla pagina di login.

## Come Provare il Progetto

Per eseguire questo progetto localmente, avrai bisogno di:
* Node.js e npm (per Angular)
* .NET SDK (per ASP.NET Core, es. .NET 6, 7 o 8)
* SQL Server (o SQL Server Express) e uno strumento per gestirlo come SQL Server Management Studio (SSMS).

**1. Setup del Backend (`/backend`)**

   a. Apri la cartella `backend` in un terminale.
   b. **Configura i segreti dell'applicazione**:
      Questo progetto usa "User Secrets" per la chiave JWT e la stringa di connessione al database (per non commettere dati sensibili nel codice). Esegui:
      ```bash
      dotnet user-secrets init
      dotnet user-secrets set "Jwt:Key" "LA_TUA_CHIAVE_SEGRETA_MOLTO_LUNGA_E_COMPLESSA_PER_HS256_MIN_32_CARATTERI_BYTES"
      dotnet user-secrets set "ConnectionStrings:DbConnection" "Data Source=TUO_SERVER_SQL;Initial Catalog=NOME_DB_JWT;Integrated Security=True;Encrypt=False;TrustServerCertificate=True"
      ```
      * Sostituisci `"LA_TUA_CHIAVE_SEGRETA_..."` con una stringa casuale lunga (almeno 32 caratteri/bytes).
      * Sostituisci `"LA_TUA_STRINGA_DI_CONNESSIONE_..."` con la stringa di connessione corretta per il tuo SQL Server. Assicurati che il database `NOME_DB_JWT` esista (puoi crearlo vuoto da SSMS).
   c. **Applica le Migrazioni del Database**:
      Questo comando creerà le tabelle necessarie nel database specificato.
      ```bash
      dotnet ef database update
      ```
      (Se è la prima volta e non ci sono migrazioni, potresti doverle creare prima: `dotnet ef migrations add InitialUserSetup`)
   d. **Avvia il Backend**:
      ```bash
      dotnet run
      ```
      L'API dovrebbe essere in esecuzione (controlla la console per l'URL `https://localhost:PORTA_BACKEND`, es. 7176).

**2. Setup del Frontend (`/frontend`)**

   a. Apri un **nuovo** terminale e naviga nella cartella `frontend`.
   b. **Installa le dipendenze**:
      ```bash
      npm install
      ```
   c. **Configura l'URL dell'API**:
      Apri il file `src/environments/environment.ts` e assicurati che la proprietà `apiUrl` punti all'URL corretto del tuo backend (quello mostrato quando hai avviato il backend, es. `apiUrl: 'https://localhost:7176/api'`).
   d. **Avvia il Frontend**:
      ```bash
      ng serve -o
      ```
      L'applicazione Angular si aprirà nel browser es. `http://localhost:4200/`.


## Note Aggiuntive

* Il token JWT ha una scadenza breve (configurata nel backend, es. 1 o 60 minuti) per motivi di sicurezza e test.
