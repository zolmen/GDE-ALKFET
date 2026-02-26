# X.509 Tanúsítványkezelő Alkalmazás — MCP Integrációval

> Mikroszolgáltatás alapú, X.509 tanúsítványkezelő alkalmazás (Root CA és Felhasználói CSR aláírás).  
> **.NET 9 Web API** backend és **Angular 19** frontend.

---

## Alkalmazás Architektúrája

A projekt három fő Backend rétegre és egy Frontend Single Page Applicationre oszlik:

| Réteg | Leírás |
|---|---|
| **CertStore.Domain** | Üzleti entitások, és adatátviteli objektumok |
| **CertStore.Infrastructure** | MongoDB alapú persistence réteg, Generikus Repository-k, UnitOfWork |
| **CertStore.API** | REST API Controllerek, Üzleti Service-ek (BouncyCastle alapú X.509 generálás), és az MCP Server (Tool-ok) |
| **WebUI (Angular)** | Kliensalkalmazás a tanúsítványok weben keresztüli letöltéséhez, menedzseléséhez és CSR fájlok beküldéséhez |

---

## Lokális Fejlesztés és Futtatás

Az alkalmazás futtatásához **`NET 9 SDK`**, **`Node.js`**, és **`Docker`** (MongoDB-hez) szükséges.

### 1️⃣ MongoDB elindítása

A backend egy MongoDB adatbázist igényel. Docker segítségével azonnal elindítható:

```bash
docker run -d -p 27017:27017 --name cert-mongo mongo:latest
```

### 2️⃣ Backend API indítása

```bash
cd CertWebApi/CertStore.API
dotnet run
```

> 📖 A REST API Swagger dokumentációja böngészőből is elérhető fejlesztői módban:  
> `http://localhost:sajatPort/swagger`

### 3️⃣ Angular WebUI indítása

```bash
cd WebUI
npm install
npm run start
```

> Az oldal a `http://localhost:4200` címen érhető el. Ha nem fut más azon a porton...

---

## MCP kipróbálása — VS Code + Claude Code

A C# Backend dedikált `/mcp` végponttal rendelkezik, amely SSE-vel működik.  
A Claude Code a **Streamable HTTP** protokollt használja, közvetlen HTTP kapcsolattal dolgozik.

### Lépések

1. Telepítsd a **Claude Code** VS Code kiterjesztést az áruházból.
2. A projekt gyökerében (ahol a `.mcp.json` fájl található) nyisd meg a projektet VS Code-ban.
3. A `.mcp.json` fájl automatikusan betöltődik, és a Claude Code felismeri a backend MCP szerverét:

```json
{
  "mcpServers": {
    "cert-store-mcp": {
      "url": "http://localhost:5181/mcp",
      "type": "streamable-http"
    }
  }
}
```

4. Győződj meg róla, hogy a backend fut, majd nyisd meg a Claude Code panelt.
5. A chatbe írd be magyarul például a feladatot:

> *"Hozz létre egy új root tanúsítványt 'Test CA' névvel, 365 napos érvényességgel!"*

---

## Microsoft Infrastruktúra, CI/CD és Kubernetes (Élesítés)

Az infrastruktúra megteremtése és üzemeltetése a projekt teljes ideje alatt. Microsoft 365 és Azure Free Tier felhős környezet kialakítása, Kubernetes cluster telepítés, CI/CD pipeline, adatbázis konfiguráció és GitOps automatizálása.

### Előfeltételek (lokális gépen)

A munkához a következő szoftverekre van szükség:

| Eszköz | Funkció |
|---|---|
| **Azure CLI** (`az`) | Azure erőforrások kezelése parancssorból |
| **kubectl** | Kubernetes cluster vezérlése |
| **Helm** | Kubernetes csomagkezelő (NGINX Ingress, cert-manager) |
| **Git** | Verziókezelés, GitHub repo kezelése |

---

### Microsoft 365 és Azure Platform

A projekt az általam korábban már meglévő Microsoft 365 felhő infrastruktúrára és egy regisztrált domain-re épült.

**Tenant beállítás:**

1. M365 Business Premium aktiválás (`admin.microsoft.com`)
2. Domain cím verifikálás TXT rekorddal a hoszting szolgáltató cPanel-ben
3. Entra ID P1 (benne a licencben) identitáskezelés, MFA, Conditional Access
4. Azure Pay-As-You-Go előfizetés létrehozása (`portal.azure.com`)

**Conditional Access szabályok:**

- Block Non-EU Countries (27 EU tagállam Named Location)
- MFA for all users / admins / Azure Management
- Block legacy authentication

---

### Azure Erőforrások

Jelen esetben 4 db resource group-ra volt szükség cost-center tag-ekkel:

```bash
az group create --name rg-karpatilabor-network --location westeurope --tags cost-center=network
az group create --name rg-karpatilabor-aks     --location westeurope --tags cost-center=compute
az group create --name rg-karpatilabor-data    --location westeurope --tags cost-center=database
az group create --name rg-karpatilabor-shared  --location westeurope --tags cost-center=shared
```

**Virtual Network:**

```bash
az network vnet create \
  --name vnet-karpatilabor-weu \
  --resource-group rg-karpatilabor-network \
  --address-prefix 192.168.0.1/24 \
  --subnet-name snet-aks \
  --subnet-prefix 192.168.0.1/24
```

---

### AKS Cluster (Kubernetes)

```bash
az aks create \
  --name aks-karpatilabor \
  --resource-group rg-karpatilabor-aks \
  --node-count 1 \
  --node-vm-size Standard_B2s_v2 \
  --tier free \
  --kubernetes-version 1.33.6 \
  --network-plugin azure \
  --vnet-subnet-id <SUBNET_ID> \
  --generate-ssh-keys
```

> **Megjegyzés:** A `Standard_B2s_v2` VM SKU-hoz kvóta emelést kellett kérvényezni az Azure Portal-on (0 → 4 vCPU).

**Csatlakozás a clusterhez:**

```bash
az aks get-credentials --resource-group rg-karpatilabor-aks --name aks-karpatilabor
```

**Namespace-ek:** `karpatilabor`, `argocd`, `ingress-nginx`, `cert-manager`

---

### DocumentDB (MongoDB)

- Free Tier, 32 GB, MongoDB 8.0 kompatibilis
- Régió: **East US** (Free Tier csak itt érhető el!)
- Firewall: `AllowAzureServices` + 3 fejlesztő IP cím hozzáadva

A Kubernetes-ben Secret tárolja a connection string-et (URL-encoded jelszóval):

```bash
kubectl create secret generic mongodb-secret \
  --namespace karpatilabor \
  --from-literal=connection-string="mongodb+srv://dbadmin:<URL_ENCODED_PW>@db-karpatilabor.mongocluster.cosmos.azure.com/?tls=true&authMechanism=SCRAM-SHA-256&retrywrites=false&maxIdleTimeMS=120000"
```

---

### NGINX Ingress Controller + Let's Encrypt

```bash
helm install ingress-nginx ingress-nginx/ingress-nginx \
  --namespace ingress-nginx \
  --create-namespace

kubectl apply -f https://github.com/cert-manager/cert-manager/releases/download/v1.17.1/cert-manager.yaml
```

- Public IP: `<TITKOS>` (Azure Load Balancer)
- Let's Encrypt tanúsítvány, ACME
- ClusterIssuer: `letsencrypt-prod`

**DNS (Hosting cPanel):**

```
cegesdomain.com  ->  A  ->  PUBLIC IP ADDRESS
```

---

### CI/CD Pipeline (GitHub Actions)

A GitHub repository: **[zolmen/GDE-ALKFET](https://github.com/zolmen/GDE-ALKFET)** (Public)

Két GitHub Actions workflow automatizálja a Docker image build-et:

| Workflow | Image |
|---|---|
| `build-backend.yaml` | `ghcr.io/zolmen/gde-alkfet/backend:latest` |
| `build-frontend.yaml` | `ghcr.io/zolmen/gde-alkfet/frontend:latest` |

> Push a `main` branch-re → GitHub Actions build → Docker image push a **GitHub Container Registry**-be (`ghcr.io`)

---

### ArgoCD (GitOps)

```bash
kubectl apply -n argocd -f https://raw.githubusercontent.com/argoproj/argo-cd/stable/manifests/install.yaml
```

Az ArgoCD figyeli a GitHub repo `k8s/` mappáját (Kustomize source) és automatikusan szinkronizálja az AKS cluster állapotát:

- **Auto-sync:** ON
- **Dashboard:** `kubectl port-forward svc/argocd-server -n argocd 8080:443` → `https://localhost:8080`

> Biztonsági okokból nincs publikálva a nagyvilágba.

---

### ArgoCD Image Updater

Az automatikus deployment utolsó lépése. Ha a fejlesztők push-olnak és a GitHub Actions új Docker image-et készít, akkor az Image Updater **2 percen belül** észleli a változást és frissíti a pod-okat.

**Konfiguráció az ArgoCD Application-ön:**

- Update strategy: `digest` (figyeli a `:latest` tag mögötti SHA256 változást)
- Registry auth: GitHub PAT token (`read:packages` scope)

---

### Teljes Deployment Lánc

```
Fejlesztők git push
       ↓
GitHub Actions (Docker build)
       ↓
ghcr.io (Container Registry)
       ↓
ArgoCD Image Updater (digest poll, ~2 perc)
       ↓
AKS pod frissítés
       ↓
HTTPS-en elérhető az új verzió
```
