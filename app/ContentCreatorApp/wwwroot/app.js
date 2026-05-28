(function () {
  function setupOpening() {
    const overlay = document.getElementById("opening-screen");
    if (!overlay || overlay.dataset.ready === "true") return;

    overlay.dataset.ready = "true";
    const button = overlay.querySelector(".opening-sound");
    const audioSrc = overlay.dataset.audioSrc;
    const audio = audioSrc ? new Audio(audioSrc) : null;
    if (audio) {
      audio.volume = 0.74;
    }

    const dismiss = () => {
      overlay.classList.add("is-done");
      setTimeout(() => overlay.remove(), 900);
    };

    const playWithSound = async () => {
      if (!audio) {
        dismiss();
        return;
      }

      try {
        audio.currentTime = 0;
        await audio.play();
        overlay.classList.add("audio-on");
        setTimeout(dismiss, 3500);
      } catch {
        overlay.classList.add("needs-interaction");
        if (button) {
          button.hidden = false;
          button.disabled = false;
        }
      }
    };

    if (button) {
      button.addEventListener("click", async () => {
        button.disabled = true;
        await playWithSound();
      });
    }

    setTimeout(playWithSound, 280);
    setTimeout(dismiss, 6200);
  }

  const preview = {
    play(id) {
      const video = document.getElementById(id);
      if (!video) return;

      video.muted = true;
      if (Number.isFinite(video.duration) && video.duration > 8 && video.currentTime < 1) {
        video.currentTime = 4;
      }

      const playPromise = video.play();
      if (playPromise) {
        playPromise.catch(() => {});
      }
    },
    stop(id) {
      const video = document.getElementById(id);
      if (!video) return;

      video.pause();
      video.currentTime = 0;
    }
  };

  function prepareVideoThumbnails() {
    document.querySelectorAll("video.cine-preview[data-thumbnail-video]").forEach((video) => {
      if (video.dataset.thumbnailReady === "true") return;
      video.dataset.thumbnailReady = "true";
      video.muted = true;

      const captureFirstFrame = () => {
        try {
          if (Number.isFinite(video.duration) && video.duration > 0) {
            video.currentTime = Math.min(0.18, Math.max(video.duration - 0.1, 0));
          }
        } catch {
          video.classList.add("thumbnail-ready");
        }
      };

      video.addEventListener("loadedmetadata", captureFirstFrame, { once: true });
      video.addEventListener("seeked", () => {
        video.pause();
        video.classList.add("thumbnail-ready");
      }, { once: true });

      if (video.readyState >= 1) {
        captureFirstFrame();
      }
    });
  }

  globalThis.paquetaPreview = preview;
  if (typeof window !== "undefined") {
    window.paquetaPreview = preview;
  }
  if (typeof document !== "undefined") {
    document.documentElement.dataset.paquetaPreview = "ready";
    if (document.readyState === "loading") {
      document.addEventListener("DOMContentLoaded", () => {
        setupOpening();
        prepareVideoThumbnails();
      }, { once: true });
    } else {
      setupOpening();
      prepareVideoThumbnails();
    }

    const openingWatcher = setInterval(setupOpening, 500);
    setTimeout(() => clearInterval(openingWatcher), 12000);
    const thumbnailWatcher = setInterval(prepareVideoThumbnails, 800);
    setTimeout(() => clearInterval(thumbnailWatcher), 20000);
  }
})();
