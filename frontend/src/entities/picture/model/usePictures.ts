import { useQuery } from "@tanstack/react-query";

export const usePictures = () => {
  const { data, isLoading, isError } = useQuery({
    queryKey: ["photos"],
    queryFn: async () => {
      const response = await fetch(`/api/photos`, { credentials: "include" });
      if (!response.ok) throw new Error("Failed to fetch photos");
      return response.json();
    },
  });

  const photos = data?.photos ?? [];

  return { photos, isLoading, isError };
};
