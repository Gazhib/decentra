import { useQuery, useQueryClient } from "@tanstack/react-query";
import { useNavigate } from "react-router-dom";
import LoadingSpinner from "../shared/loading-spinner/ui/LoadingSpinner";

type Appeal = { id: string; title: string };

function useAppeals() {
  return useQuery<Appeal[]>({
    queryKey: ["appeals"],
    queryFn: async () => {
      const response = await fetch("/api/appeals", { credentials: "include" });
      if (!response.ok) throw new Error(`HTTP ${response.status}`);
      const responseData = await response.json();
      console.log(responseData);
      return responseData;
    },
    staleTime: 5 * 60 * 1000,
    refetchOnWindowFocus: false,
    retry: 1,
  });
}

export default function AdminPage() {
  const { data, isLoading, isError, error, refetch } = useAppeals();
  const qc = useQueryClient();
  const navigate = useNavigate();

  const prefetchAppeal = (id: string) =>
    qc.prefetchQuery({
      queryKey: ["appeal", id],
      queryFn: async () => {
        const response = await fetch(`/api/appeals/${id}`, {
          credentials: "include",
        });
        const responseData = await response.json();
        if (!response.ok) throw new Error(`HTTP ${response.status}`);
        console.log(responseData);
        return responseData;
      },
      staleTime: 60_000,
    });

  if (isLoading) return <LoadingSpinner />;
  if (isError)
    return (
      <div>
        Failed: {(error as Error).message}{" "}
        <button onClick={() => refetch()}>Retry</button>
      </div>
    );
  if (!data?.length) return <div>No appeals</div>;

  return (
    <main className="min-h-full">
      <h1>Admin Page</h1>
      <ul>
        {data.map((a) => (
          <li key={a.id}>
            <button
              onMouseEnter={() => prefetchAppeal(a.id)}
              onClick={() => navigate(`/appeals/${a.id}`)}
            >
              {a.title}
            </button>
          </li>
        ))}
      </ul>
    </main>
  );
}
