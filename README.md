# NeuroDoom-Unity

Este repositorio contiene el prototipo en Unity de **NeuroDoom**, desarrollado en el marco del Trabajo Fin de Máster:
*Ajuste dinámico de la dificultad en videojuegos basado en estados emocionales del jugador mediante dispositivos BCI*.

Se trata de un videojuego retro-FPS adaptativo que integra un sistema de **ajuste dinámico de la dificultad (DDA)** guiado por señales EEG recogidas con NeuroSky. 

---

## 🎮 Descripción
Este repositorio contiene el prototipo jugable **NeuroDoom**, inspirado en los FPS retro.  
Incluye tres modos de ajuste dinámico de dificultad (DDA):  
- **Preconfigurado:** parámetros fijos definidos a priori.  
- **Árbol heurístico:** reglas explícitas basadas en la detección de estados afectivos.  
- **Bandit ML:** modelo contextual con Thompson Sampling para predicción proactiva de estados.  

La comunicación en tiempo real con el módulo [neurodoom-python](https://github.com/nooaahgl/eeg-dda_neurodoom-py) se realiza mediante UDP, lo que permite sincronizar las métricas EEG con las condiciones de juego.

---

## ✨ Funcionalidades principales
- **Tres modos de adaptación de dificultad**
- **Integración UDP**: comunicación en tiempo real con el módulo Python para sincronizar métricas EEG y eventos de juego.  
- **Escenarios retro-FPS**: entorno de hordas inspirado en shooters clásicos.  
- **Soporte multi-sesión**: cada partida genera logs de eventos, estados y resultados de puntuación.  

---

## ⚙️ Requisitos
- **Unity 6000.0.49f1 LTS** (otras versiones LTS 2022 pueden ser compatibles).
- **Dispositivo NeuroSky Brainwave Starter Kit** 
- **ThinkGear Connector** instalado y en ejecución para lectura del NeuroSky.  
- Conexión con `neurodoom-python` mediante UDP en localhost.

---
## ✨ Clonar el repositorio
git clone https://github.com/nooaahGL/eeg-dda_neurodoom-unity.git
cd neurodoom-python

---

## 📜 Licencia
Este proyecto se distribuye bajo licencia **MIT**.
