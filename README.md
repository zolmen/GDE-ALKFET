Alkalmazás publikus elérése:

https://karpatilabor.com/

-------------------------------------------------

X.509 Tanúsítványkezelő Alkalmazás MCP Integrációval:

Ez a projekt egy mikroszolgáltatás alapú, X.509 tanúsítványkezelő alkalmazás (Root CA és Felhasználói CSR aláírás). .NET 9 Web API backendből és egy Angular 19 frontendből áll. 

 Alkalmazás Architektúrája

A projekt három fő Backend rétegre és egy Frontend Single Page Applicationre oszlik:
1. CertStore.Domain:Üzleti entitások, és adatátviteli objektumok
2. CertStore.Infrastructure:** MongoDB alapú persistence réteg, Generikus Repository-k, UnitOfWork.
3. CertStore.API:** REST API Controllerek, Üzleti Service-ek (BouncyCastle alapú X.509 generálás), és az MCP Server (Tool-ok).
4. WebUI (Angular):** Kliensalkalmazás a tanúsítványok weben keresztüli letöltéséhez, menedzseléséhez és CSR fájlok beküldéséhez.

---

Lokális Fejlesztés és Futtatás

Az alkalmazás futtatásához .NET 9 SDK, Node.js, és Docker (MongoDB-hez) szükséges.

1. MongoDB elindítása
A backend egy MongoDB adatbázist igényel. Docker segítségével azonnal elindítható:

docker run -d -p 27017:27017 --name cert-mongo mongo:latest

2. Backend API indítása

cd CertWebApi/CertStore.API
dotnet run

A REST API Swagger dokumentációja böngészőből is elérhető fejlesztői módban: `http://localhost:sajatPort/swagger`*

3. Angular WebUI indítása

cd WebUI
npm install
npm run start

Az oldal a `http://localhost:4200` címen érhető el. Ha nem fut más azona porton... 



MCP kipróbálása VS Code + Claude Code

A C# Backend dedikált `/mcp` végponttal rendelkezik, amely SSE vel mukodik

A Claude Code a Streamable HTTP protokollt használja, közvetlen HTTP kapcsolattal dolgozik.

1. Telepítsd a Claude Code VS Code kiterjesztést az áruházból.
2. A projekt gyökerében (ahol a .mcp.json fájl található) nyisd meg a projektet VS Code-ban.
3. A .mcp.json fájl automatikusan betöltődik, és a Claude Code felismeri a backend MCP szerverét:
```json
{
  "mcpServers": {
    "cert-store-mcp": {
      "url": "http://localhost:5181/mcp", //nyilván saját portal....
      "type": "streamable-http"
    }
  }
}
```
4. Győződj meg róla, hogy a backend fut, majd nyisd meg a Claude Code panelt.
5. A chatbe írd be magyarul például hogy a feladatot: "Hozz létre egy új root tanúsítványt 'Test CA' névvel, 365 napos érvényességgel!"

---

## ☁️ CI/CD és Kubernetes (Élesítés)

A szoftver tartalmaz összeállított DevOps konfigurációkat egy felhős (K8s) élesítésez.

### Github Actions Pipiline
A projekt tartalmazza a `.github/workflows/docker-image.yml` állományt. Amikor felküldöd a kódot a GitHub `master` branch-re, a folyamat automatikusan lefut:
1. Felépíti a C# backend Docker image-t és felölti a Github Container Registrybe (`ghcr.io/te-github-neved/gde-backend`).
2. Felépíti az Angular Nginx Docker image-t és feltölti a regisztrációba.

### Kubernetes Telepítés (K8s)
Miután a Docker image-ek a regisztrációdban (GHCR) vannak, módosítsd a `k8s/vackend.yaml` és `k8s/frontend.yaml` fájlok `image:` sorát a saját Github Registry útvonaladra.

Ezután a három szolgáltatás a következő parancsokkal indítható a K8s Clusteredben:
```bash
kubectl apply -f k8s/mongo.yaml
kubectl apply -f k8s/backend.yaml
kubectl apply -f k8s/frontend.yaml
```
*A K8s Config biztosítja, hogy a Backend `mongodb://mongo:27017` connection string segítségével eléri az elszigetelt mongoDb POD-ot, a backend és a frontend load ballanszerek pedig felélednek az internetre.*
