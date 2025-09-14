import { useQuery, useQueryClient } from "@tanstack/react-query";
import { useNavigate } from "react-router-dom";
import LoadingSpinner from "../shared/loading-spinner/ui/LoadingSpinner";

type Appeal = {
  appeals: {
    appealed: boolean;
    description: string;
    id: number;
    photoIds: number[];
    userId: number;
  }[];
};

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

  if (isLoading)
    return (
      <main className="min-h-screen bg-gray-50">
        <div className="max-w-5xl mx-auto px-4 sm:px-6 lg:px-8 py-16 flex items-center justify-center">
          <LoadingSpinner />
        </div>
      </main>
    );

  if (isError)
    return (
      <main className="min-h-screen bg-gray-50">
        <div className="max-w-2xl mx-auto px-4 sm:px-6 lg:px-8 py-16">
          <div className="rounded-xl border border-red-200 bg-red-50 p-6 text-red-800">
            <div className="flex items-center justify-between gap-4">
              <p className="font-medium">Failed: {(error as Error).message}</p>
              <button
                onClick={() => refetch()}
                className="inline-flex items-center rounded-lg bg-red-600 px-3 py-1.5 text-white text-sm font-medium shadow-sm hover:bg-red-700 focus:outline-none focus-visible:ring-2 focus-visible:ring-red-500"
              >
                Retry
              </button>
            </div>
          </div>
        </div>
      </main>
    );

  if (data?.length === 0)
    return (
      <main className="min-h-screen bg-gray-50">
        <div className="max-w-3xl mx-auto px-4 sm:px-6 lg:px-8 py-20">
          <div className="rounded-xl border border-gray-200 bg-white p-10 text-center shadow-sm">
            <p className="text-lg font-medium text-gray-900">Нет аппеляций</p>
            <p className="mt-1 text-sm text-gray-500">
              Похоже, что список пуст.
            </p>
          </div>
        </div>
      </main>
    );

  return (
    <main className="min-h-screen bg-gray-50">
      <div className="max-w-5xl mx-auto px-4 sm:px-6 lg:px-8 py-10">
        <header className="mb-8">
          <h1 className="text-2xl font-semibold text-gray-900">Аппеляции</h1>
          <p className="mt-1 text-sm text-gray-500">
            Выберите аппеляцию для просмотра деталей.
          </p>
        </header>

        <ul className="grid gap-4 sm:grid-cols-2">
          {data &&
            data?.appeals?.map((a) => (
              <li key={a.id} className="group">
                <button
                  onMouseEnter={() => prefetchAppeal(a.id)}
                  onClick={() => navigate(`/admin/appeals/${a.id}`)}
                  className="block w-full rounded-xl border border-gray-200 bg-white p-4 text-left shadow-sm transition hover:shadow-md focus:outline-none focus-visible:ring-2 focus-visible:ring-indigo-500"
                >
                  <div className="flex items-center justify-between gap-3">
                    <div className="min-w-0">
                      <p className="font-medium text-gray-900 truncate">
                        {a.description}
                      </p>
                      <p className="mt-1 text-sm text-gray-500">ID: {a.id}</p>
                    </div>
                    <span className="shrink-0 text-indigo-600 opacity-0 transition group-hover:opacity-100">
                      →
                    </span>
                  </div>
                </button>
              </li>
            ))}
        </ul>
      </div>
    </main>
  );
}
