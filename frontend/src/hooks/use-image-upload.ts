import { useState } from "react";
import imageCompression from "browser-image-compression";
import { postJson } from "../lib/api";

interface UploadUrlResponse {
  signedUrl: string;
  publicUrl: string;
  path: string;
}

interface UseImageUploadResult {
  uploadImage: (file: File, bucket: "avatars" | "event-images") => Promise<{ publicUrl: string; path: string }>;
  isUploading: boolean;
  error: string | null;
}

export function useImageUpload(): UseImageUploadResult {
  const [isUploading, setIsUploading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  async function uploadImage(file: File, bucket: "avatars" | "event-images") {
    setIsUploading(true);
    setError(null);

    try {
      // Compress image
      const options = {
        maxWidthOrHeight: bucket === "avatars" ? 300 : 1200,
        useWebWorker: true,
        fileType: "image/jpeg",
      };

      const compressedFile = await imageCompression(file, options);

      const contentType = compressedFile.type || "image/jpeg";
      const fileName = compressedFile.name || "image.jpg";

      // Get signed URL from backend
      const uploadData = await postJson<UploadUrlResponse>("/api/images/upload-url", {
        bucket,
        fileName,
        contentType,
      });

      // Upload directly to Supabase Storage via signed URL
      const uploadResponse = await fetch(uploadData.signedUrl, {
        method: "PUT",
        headers: {
          "Content-Type": contentType,
        },
        body: compressedFile,
      });

      if (!uploadResponse.ok) {
        throw new Error("Falha ao fazer upload da imagem.");
      }

      return { publicUrl: uploadData.publicUrl, path: uploadData.path };
    } catch (err: any) {
      const message = err?.message || "Erro ao fazer upload da imagem.";
      setError(message);
      throw err;
    } finally {
      setIsUploading(false);
    }
  }

  return { uploadImage, isUploading, error };
}
