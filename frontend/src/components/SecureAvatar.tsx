import { api } from '@/lib/apiClient';
import { useEffect, useState } from 'react';

interface SecureAvatarProps {
  avatarUrl: string;
  sessionToken: string;
  alt: string;
  className?: string;
  size?: number;
}

/**
 * SecureAvatar Component
 *
 * Fetches authenticated images as blobs and renders them via object URLs.
 * This prevents exposing authentication tokens in the <img> src attribute.
 *
 * @param avatarUrl - The external URL of the avatar image (e.g., from Moodle/Blackboard)
 * @param sessionToken - The bb_session authentication token
 * @param alt - Alt text for the image
 * @param className - Additional CSS classes
 * @param size - Image size in pixels (width and height)
 */
export default function SecureAvatar({
  avatarUrl,
  sessionToken,
  alt,
  className = '',
  size = 40,
}: SecureAvatarProps) {
  const [imageSrc, setImageSrc] = useState<string | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(false);

  useEffect(() => {
    let objectUrl: string | null = null;
    let isMounted = true;

    const fetchImage = async () => {
      try {
        setLoading(true);
        setError(false);

        // Fetch image as blob using the Swagger-generated API client
        const response = await api.api.imageProxyList(
          {
            imageUrl: avatarUrl,
            // Token is passed via header (preferred method)
          },
          {
            headers: {
              'X-Session-Cookie': sessionToken,
            },
            // Ensure response is treated as blob
            format: 'blob',
          },
        );

        // Check if component is still mounted before updating state
        if (!isMounted) return;

        // Convert blob to object URL
        if (response.data) {
          // Type assertion since API client returns blob format
          const blob = response.data as unknown as Blob;
          objectUrl = URL.createObjectURL(blob);
          setImageSrc(objectUrl);
        } else {
          throw new Error('Invalid response format');
        }
      } catch (err) {
        console.error('Failed to load avatar image:', err);
        if (isMounted) {
          setError(true);
        }
      } finally {
        if (isMounted) {
          setLoading(false);
        }
      }
    };

    // Only fetch if we have required props
    if (avatarUrl && sessionToken) {
      fetchImage();
    } else {
      setError(true);
      setLoading(false);
    }

    // Cleanup function to revoke object URL and prevent memory leaks
    return () => {
      isMounted = false;
      if (objectUrl) {
        URL.revokeObjectURL(objectUrl);
      }
    };
  }, [avatarUrl, sessionToken]);

  // Get initials from alt text for fallback
  const getInitials = (name: string): string => {
    const words = name.trim().split(/\s+/);
    if (words.length >= 2) {
      return (words[0][0] + words[words.length - 1][0]).toUpperCase();
    }
    return name.substring(0, 2).toUpperCase();
  };

  // Loading state - pulse skeleton
  if (loading) {
    return (
      <div
        className={`bg-gray-300 rounded-full animate-pulse ${className}`}
        style={{ width: size, height: size }}
        aria-label="Loading avatar"
      />
    );
  }

  // Error state - fallback to initials
  if (error || !imageSrc) {
    return (
      <div
        className={`bg-linear-to-br from-blue-500 to-purple-600 text-white rounded-full flex items-center justify-center font-semibold ${className}`}
        style={{ width: size, height: size, fontSize: size / 2.5 }}
        aria-label={alt}
      >
        {getInitials(alt)}
      </div>
    );
  }

  // Success state - render image with object URL
  return (
    <img
      src={imageSrc}
      alt={alt}
      className={`rounded-full object-cover ${className}`}
      style={{ width: size, height: size }}
      onError={() => setError(true)}
    />
  );
}
