# 🖥️ OmniWeigh

**OmniWeigh** is a modern, high-performance industrial desktop application designed to automate, streamline, and secure metrological weighing operations[cite: 1]. It interfaces directly with hardware weighing terminals via serial communication to capture weight data in real-time, eliminating manual data entry errors, omissions, and operational fraud[cite: 1].

The software is engineered with a strict modular monolithic architecture (**Modulith**), ensuring clear domain boundaries, local reliability, and a seamless path toward future cloud-synchronized deployments[cite: 1].

---

## 🚀 Functional Architecture & Key Features

OmniWeigh is split into highly isolated functional modules, each managing a dedicated business domain of the industrial weighing ecosystem:

* **Weighing Domain:** Handles real-time metrological acquisition from hardware scale indicators (RS232/USB) and tracks stable weight states automatically[cite: 1].
* **Document & Logging Domain:** Manages local data persistence, historical logging, and the generation of standardized weight tickets and Delivery Notes (Bons de Livraison).
* **Identity & Security Domain:** Enforces hardware-bound licensing verification tightly coupled with the unique fingerprint (Hardware ID - HWID) of the deployment workstation to prevent unauthorized execution or tampering[cite: 1].

---

## 🛠️ Global Tech Stack

* **Platform:** .NET 10
* **Presentation Layer:** Windows Presentation Foundation (WPF) / MVVM Pattern[cite: 1]
* **Architecture Style:** Modulith (Modular Monolith)[cite: 1]
* **Hardware Interface:** Serial Port Protocols (RS232 / USB)[cite: 1]

---

## 🛠️ Support & Maintenance (SAV)

We believe in maximum operational reliability for industrial environments. OmniWeigh comes with a **lifetime paid technical support and maintenance service (SAV)**. This ensures your weighing stations receive priority troubleshooting, remote assistance, and total compliance with future OS upgrades. Contact the author or authorized commercial partners to establish a formal Service Level Agreement (SLA).

---

## 🛡️ License & Commercial Inquiries

OmniWeigh is **Proprietary Software**. All rights reserved by the author (**Granix**)[cite: 1]. 

The hosting of this source code on a public GitHub repository does not grant any right to copy, modify, distribute, sublicense, or use the application for commercial, industrial, or personal deployment without an official paid license[cite: 1]. 

* For purchasing official per-scale licenses, requesting custom industrial modules, or scheduling deployments, please submit an inquiry through official distribution channels or contact the repository owner.
* Review the full terms in the accompanying `LICENSE` file.