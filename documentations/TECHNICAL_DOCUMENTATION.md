# 🛠️ OmniWeigh Technical Documentation

Welcome to the technical documentation for **OmniWeigh**. This document describes the architecture, component structure, data models, hardware communication protocols, and sequence flows of the application.

---

## 🏗️ Architecture Overview

OmniWeigh is designed as a **Modular Monolith (Modulith)**, separating concerns into distinct logical domains while deploying as a single desktop application. This design provides high reliability in industrial environments, ensuring local autonomy while keeping the codebase maintainable and prepared for potential cloud synchronization.

```mermaid
graph TD
    subgraph Presentation Layer [OmniWeigh.Desktop]
        Views[Views - XAML] --> ViewModels[ViewModels - C#]
    end

    subgraph Core Business Layer [OmniWeigh.Core]
        ViewModels --> Services[Services & DTOs]
        ViewModels --> Drivers[Hardware Drivers]
        Services --> Models[Domain Models]
        Services --> Data[Data Access - EF Core & SQLite]
    end
    
    subgraph External Interfaces
        Drivers -->|RS232/USB Serial| HardwareScale[Physical Scale Indicator]
        Data -->|Local SQLite Engine| LocalDB[omniweigh.db]
    end
    
    style PresentationLayer fill:#f9f,stroke:#333,stroke-width:2px
    style CoreBusinessLayer fill:#bbf,stroke:#333,stroke-width:2px
    style ExternalInterfaces fill:#dfd,stroke:#333,stroke-width:2px
```

### 1. Presentation Layer (`OmniWeigh.Desktop`)
Developed using **Windows Presentation Foundation (WPF)** on **.NET 10**, applying the **MVVM (Model-View-ViewModel)** design pattern.
* **Views:** Define the UI layouts and elements in XAML, utilizing centralized styles (`Themes/`) and value converters (`Converters/`).
* **ViewModels:** Manage screen states, input validation, and user commands (e.g. `RelayCommand`), communicating directly with core services via Dependency Injection.

### 2. Core Business Layer (`OmniWeigh.Core`)
Encapsulates business operations, data persistence, and hardware device drivers.
* **Drivers:** Handle serial communication (RS-232 / USB) with weighing terminals.
* **Services:** Manage CRUD operations, business rules, and mapping of entities to Data Transfer Objects (DTOs).
* **Models:** Define core domain entities (e.g., Client, Product, Vehicle, Weighing).
* **Data Access:** Orchestrated by EF Core, persisting data into a local SQLite database file located in the user's `LocalApplicationData` directory.

---

## 🔌 Hardware Interface & Drivers

Industrial scales broadcast weight measurements as character streams via serial connections. The driver subsystem provides real-time metrological data acquisition.

```mermaid
classDiagram
    class IBalanceDriver {
        <<interface>>
        +String BrandName
        +String ModelName
        +Boolean IsConnected
        +WeightReceived EventHandler~Double~
        +WeightReadingReceived EventHandler~WeightReading~
        +ConnectionError EventHandler~String~
        +ConnectedAsync(portName, baudRate) Task
        +DisconnectAsync() Task
    }
    class GenericAsciiBalanceDriver {
        -SerialPort _serialPort
        -Regex _weightRegex
        +ConnectedAsync(portName, baudRate) Task
        +DisconnectAsync() Task
    }
    class MockBalanceDriver {
        -PeriodicTimer _timer
        -Double _targetWeight
        -Double _currentWeight
        +ConnectedAsync(portName, baudRate) Task
        +SimulateNewWeight(weight) Void
    }
    class WeightReading {
        <<record>>
        +Double Value
        +Boolean IsStable
        +String RawFrame
    }

    IBalanceDriver <|.. GenericAsciiBalanceDriver
    IBalanceDriver <|.. MockBalanceDriver
    IBalanceDriver ..> WeightReading : uses
```

* **[IBalanceDriver](OmniWeigh.Core/Drivers/IBalanceDriver.cs):** The interface defining capabilities, event triggers, and connection states. Supports both legacy simple double event (`WeightReceived`) and metadata-rich record stream (`WeightReadingReceived`).
* **[GenericAsciiBalanceDriver](OmniWeigh.Core/Drivers/GenericAsciiBalanceDriver.cs):** Connects to physical terminals using `System.IO.Ports.SerialPort`. It parses incoming ASCII weight streams using Regular Expressions (`[-+]?[0-9]*\.?[0-9]+`) and raises events on new readings.
* **[MockBalanceDriver](OmniWeigh.Core/Drivers/MockBalanceDriver.cs):** A simulated driver used for development, testing, and demonstration. It uses a `PeriodicTimer` to emulate physical damping and weight transitions.

---

## 📈 Key Sequence Flows

### 1. Scale Initialization & Real-Time Weight Capture
When the application starts, it registers handlers to the balance driver, opens the serial connection, and streams weight updates asynchronously.

```mermaid
sequenceDiagram
    autonumber
    participant UI as MainWindow View
    participant VM as WeighingViewModel
    participant Driver as GenericAsciiBalanceDriver
    participant Port as SerialPort

    UI->>VM: InitializeAsync()
    VM->>Driver: Register event handlers
    VM->>Driver: ConnectedAsync("COM5", 9600)
    Driver->>Port: Open()
    Port-->>Driver: DataReceived Event
    Driver->>Port: ReadLine()
    Port-->>Driver: "ST,GS,   125.40 kg\r\n"
    Driver->>Driver: Parse numeric value (125.40) & check stability
    Driver-->>VM: WeightReceived / WeightReadingReceived
    VM->>VM: Update internal weight states (stability, net weight)
    VM-->>UI: PropertyChanged Notifications (triggers UI updates)
```

### 2. Registering a New Client / Product / Vehicle
Entities are registered locally through dedicated dialogs, validated, saved in SQLite, and dynamically updated in the UI tables.

```mermaid
sequenceDiagram
    autonumber
    participant UI as PriseDePoidsView
    participant VM as WeighingViewModel
    participant ClientWin as NewClientWindow
    participant ClientVM as NewClientViewModel
    participant Service as ClientService
    participant DB as OmniDbContext

    UI->>VM: Trigger OpenNewClientCommand
    VM->>ClientWin: Instantiate & ShowDialog()
    ClientWin->>ClientVM: Initialize Fields
    Note over ClientVM: User enters details & clicks Save
    ClientVM->>ClientVM: Set IsSaved = True
    ClientWin-->>VM: Close Dialog
    VM->>VM: Check Dialog Result (True & IsSaved)
    VM->>Service: AddAsync(ClientDto)
    Service->>DB: SaveChangesAsync() (Inserts entity, gets Id)
    Service->>DB: Update Reference field ("C-0000X") & SaveChangesAsync()
    Service-->>VM: Return updated ClientDto
    VM->>VM: Add to ClientsList (ObservableCollection)
    VM-->>UI: Grid updates automatically
```

### 3. Article Weighing & Registration Sequence
Below is the sequence diagram illustrating how the system handles the weighing of an article, tare deduction, net calculation, and logging.

```mermaid
sequenceDiagram
    autonumber
    participant UI as PriseDePoidsView
    participant VM as WeighingViewModel
    participant Driver as IBalanceDriver
    participant DB as OmniDbContext

    Note over UI, VM: Article is placed on scale
    Driver-->>VM: WeightReceived Event (Gross Weight = 125.40 kg)
    VM->>VM: Update CurrentWeight & check Stability
    VM-->>UI: Refresh Gross Weight readout on UI
    Note over UI: Operator selects Client, Product, & Transport Vehicle
    Note over UI: Operator inputs packaging/container Tare (e.g., 5.40 kg)
    UI->>VM: Set Tare property
    VM->>VM: Compute NetWeight (Gross - Tare = 120.00 kg)
    VM-->>UI: Refresh Net Weight readout on UI
    UI->>VM: Trigger EnregistrerCommand
    Note over VM: Build Weighing record (Timestamp, ClientId, ProductId, Gross, Tare)
    VM->>DB: Persist Weighing transaction
    DB-->>VM: Success confirmation
    Note over VM: Print ticket or Delivery Note (Bon de Livraison)
```

---

## 💾 Data Model & Storage

The database layer is managed by Entity Framework Core targeting a local **SQLite** database (`omniweigh.db`). The database is isolated within the user's `LocalAppData` directory (e.g. `C:\Users\<Name>\AppData\Local\OmniWeigh\omniweigh.db`).

### Schema Diagram

```mermaid
erDiagram
    CLIENT ||--o{ WEIGHING : has
    PRODUCT ||--o{ WEIGHING : has
    VEHICLE {
        int Id PK
        string Registration
        string Type
        string MaxLoad
        string ImageFile
    }
    CLIENT {
        int Id PK
        string Reference
        string Name
        string ContactInfo "JSON field"
    }
    PRODUCT {
        int Id PK
        string Barcode "Stores image filename"
        string Name
    }
    WEIGHING {
        int Id PK
        DateTime Timestamp
        double GrossWeight
        double Tare
        int ClientId FK
        int ProductId FK
    }
```

* **Dynamic Columns and Migrations:** The codebase implements on-the-fly table checks. For example, `EnsureReferenceColumnExistsAsync` in `ClientService.cs` inspects database metadata using SQLite `PRAGMA table_info` and dynamically runs `ALTER TABLE` to inject columns if they are absent on older workstations.

---

## 🔒 Identity & Security (License Verification)

As outlined in the software licensing agreement, OmniWeigh enforces hardware-bound licensing verification.

```mermaid
flowchart TD
    Start[Application Startup] --> GetHWID[Gather System Hardware ID - Fingerprint]
    GetHWID --> LoadLicense[Read License File from Directory]
    LoadLicense --> DecryptLicense[Decrypt and Validate Signature]
    DecryptLicense --> CheckMatch{Fingerprint matches HWID?}
    CheckMatch -- Yes --> GrantAccess[Allow Application Launch]
    CheckMatch -- No --> DenyAccess[Display License Error & Exit]
```

* **Workstation Fingerprinting:** The system queries motherboard serials, CPU IDs, and MAC addresses to compile a unique **Hardware ID (HWID)**.
* **Signature Cryptography:** Valid licenses are cryptographically signed using public/private key pairs to prevent tampering or key bypass.
* **Integrity Enforcement:** The application terminates execution if any licensing constraints are violated.
