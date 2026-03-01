import { useEffect, useState } from 'react';
import { useQuery } from '@tanstack/react-query';
import { configService } from '@/services/apiService';

export function BackgroundImages() {
  const [currentImageIndex, setCurrentImageIndex] = useState(0);

  const { data: bgResponse } = useQuery({
    queryKey: ['background-images'],
    queryFn: configService.getBackgroundImages,
    staleTime: 1000 * 60 * 60, // 1 hour
  });

  // Unwrap the API response
  const bgData = (bgResponse as any)?.data;

  useEffect(() => {
    if (!bgData || !bgData.images || bgData.images.length === 0) return;

    const interval = setInterval(() => {
      setCurrentImageIndex((prev) => (prev + 1) % bgData.images.length);
    }, (bgData.rotationInterval || 30) * 1000);

    return () => clearInterval(interval);
  }, [bgData]);

  if (!bgData || !bgData.images || bgData.images.length === 0) {
    return null;
  }

  return (
    <div className="fixed inset-0 z-0">
      {bgData.images.map((image: string, index: number) => (
        <div
          key={image}
          className={`absolute inset-0 bg-cover bg-center transition-opacity duration-1000 ${
            index === currentImageIndex ? 'opacity-100' : 'opacity-0'
          }`}
          style={{ backgroundImage: `url(${image})` }}
        />
      ))}
    </div>
  );
}
