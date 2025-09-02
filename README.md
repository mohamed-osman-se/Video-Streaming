<img width="64" height="64" alt="play-button" src="https://github.com/user-attachments/assets/1f9669f5-0f6a-4cc8-bc5c-1b6044118537" />

# Video Streaming Platform  
**Is a** backend-driven video streaming web application where users can upload videos, which are then processed using **FFmpeg**, converted into **HLS (.ts/.m3u8)** format, and streamed seamlessly.  

This project was built to demonstrate **backend engineering skills** such as background processing, queue management and scalable video transcoding.  

---

🔗 **Live Demo (HTTPS Enabled):**  
*Add link here when deployed*  

---


## 📸 Screenshots  


---

## 🧠 Why This Project?  

I created this project to learn and showcase how to build a **production-ready video streaming service** similar to YouTube or Vimeo at a smaller scale.  

The project highlights:  

✅ **Background queue with workers** to offload video processing  
✅ **Video status tracking** (Ready, Processing, Failed)  
✅ **HLS streaming support** for adaptive playback  
✅ **Docker-ready** for consistent deployments  

---

## 🛠️ Technologies Used  

| Area            | Technology                           |  
|-----------------|--------------------------------------|  
| Language        | C# (.NET 9 SDK)                      |  
| Framework       | ASP.NET Core Web API / MVC           |  
| Background Jobs | `BackgroundService` + custom queue   |  
| Video Encoding  | FFmpeg (HLS transcoding)             |  
| Database        | SQLite (via Entity Framework Core)   |     
| Containerization| Docker                               |  

---

## 📦 Prerequisites  

- .NET 9 SDK or later  
- FFmpeg installed (`ffmpeg` available in PATH)  
- SQLite  
- Docker (optional, for containerized deployment)  

---

## ⚡ Getting Started  

### Run Locally  

```bash
# Clone the repository
git clone https://github.com/mohamed-osman-se/Video-Streaming.git
cd Video-Streaming

# Restore dependencies
dotnet restore

# Run the app
dotnet run
```

---

### Run With Docker

```bash
git clone https://github.com/mohamed-osman-se/Video-Streaming.git
cd Video-Streaming

docker build -t vs-app .
docker run -p 8080:80 vs-app
