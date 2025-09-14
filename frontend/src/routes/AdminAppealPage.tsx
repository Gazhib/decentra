import { useNavigate, useParams } from "react-router-dom";
import { useQuery, useMutation } from "@tanstack/react-query";

type Appeal = {
  id: number | string;
  description?: string;
  appealed?: boolean | null;
  createdAt?: string;
  updatedAt?: string;
  photos?: Photo[]; 
};

type Photo = {
  photoId: number;
  image: string;
  dust: string;
  rust: string;
  scratch: string;
  dent: string;
};

const getAppeal = async (id: string): Promise<Appeal> => {
  const response = await fetch(`/api/appeals/${id}`, { credentials: "include" });
  if (!response.ok) throw new Error(`HTTP ${response.status}`);
  const responseData = await response.json();
  return (responseData.appeal ?? responseData) as Appeal;
};

export default function AdminAppealPage() {
  const { appealId = "" } = useParams();
  const navigate = useNavigate();

  const {
    data: appeal,
    isLoading,
    isError,
    error,
    refetch,
  } = useQuery({
    queryKey: ["appeal", appealId],
    queryFn: () => getAppeal(appealId),
    enabled: !!appealId,
    staleTime: 60 * 1000,
  });

  const updateStatus = useMutation({
    mutationFn: async (appealed: boolean) => {
      const r = await fetch(`/api/appeals/${appealId}/status`, {
        method: "PATCH",
        credentials: "include",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({ appealed }),
      });
      if (!r.ok) throw new Error(`HTTP ${r.status}`);
      return r.json();
    },
    onSuccess: () => refetch(),
  });

  const statusText =
    appeal?.appealed === true
      ? "Положительно"
      : appeal?.appealed === false
      ? "Отрицательно"
      : "На рассмотрении";

  const statusLocked =
    appeal?.appealed !== null && appeal?.appealed !== undefined;

  if (isLoading)
    return (
      <main className="min-h-screen w-full flex items-center justify-center">
        <div className="size-10 rounded-full border-4 border-gray-300 border-t-gray-900 animate-spin" />
      </main>
    );

  if (isError || !appeal)
    return (
      <main className="min-h-screen w-full p-6 flex flex-col items-center gap-4">
        <h2 className="text-2xl font-bold">Апелляция</h2>
        <p className="text-red-600">
          {(error as Error)?.message || "Не найдено"}
        </p>
        <div className="flex gap-3">
          <button onClick={() => refetch()} className="btn">
            Повторить
          </button>
          <button onClick={() => navigate(-1)} className="btn">
            Назад
          </button>
        </div>
      </main>
    );

  return (
    <main className="min-h-screen w-full max-w-5xl mx-auto p-6 flex flex-col gap-6">
      <div className="flex items-center justify-between">
        <h2 className="text-2xl font-bold">Детали апелляции</h2>
        <div className="flex gap-2">
          <button onClick={() => refetch()} className="btn">
            Обновить
          </button>
          <button onClick={() => navigate(-1)} className="btn">
            Назад
          </button>
        </div>
      </div>

      <section className="rounded-2xl border p-5 grid gap-4">
        <div className="grid grid-cols-[160px_1fr] gap-3 items-start">
          <div className="font-semibold">ID</div>
          <div>{appeal.id}</div>

          <div className="font-semibold">Создано</div>
          <div>
            {appeal.createdAt
              ? new Date(appeal.createdAt).toLocaleString()
              : "-"}
          </div>

          {appeal.updatedAt && (
            <>
              <div className="font-semibold">Обновлено</div>
              <div>{new Date(appeal.updatedAt).toLocaleString()}</div>
            </>
          )}

          <div className="font-semibold">Статус</div>
          <div>{statusText}</div>

          <div className="font-semibold">Причина</div>
          <div className="whitespace-pre-wrap">{appeal.description || "-"}</div>
        </div>
      </section>

      <section className="rounded-2xl border p-5">
        <div className="flex items-center gap-3">
          <button
            disabled={updateStatus.isPending || statusLocked}
            onClick={() => updateStatus.mutate(true)}
            className="px-4 py-2 rounded-xl bg-green-600 text-white hover:bg-green-500 disabled:opacity-60"
          >
            {updateStatus.isPending ? "Подтверждаю…" : "Подтвердить"}
          </button>
          <button
            disabled={updateStatus.isPending || statusLocked}
            onClick={() => updateStatus.mutate(false)}
            className="px-4 py-2 rounded-xl bg-rose-600 text-white hover:bg-rose-500 disabled:opacity-60"
          >
            {updateStatus.isPending ? "Отклоняю…" : "Отклонить"}
          </button>
        </div>
      </section>

      {appeal.photos?.length ? (
        <section className="rounded-2xl border p-5">
          <div className="font-semibold mb-3">Фотографии</div>

          <div className="grid grid-cols-2 md:grid-cols-4 gap-4">
            {appeal.photos?.map(
              ({ dust, rust, scratch, dent, image, photoId }) => (
                <figure key={photoId} className="flex flex-col gap-2">
                  <img
                    src={`data:image/jpeg;base64,${image}`}
                    alt={`photo ${photoId}`}
                    className="w-full h-40 object-cover rounded-xl border"
                  />
                  <figcaption className="text-xs text-gray-600">
                    Пыль: {dust} · Ржавчина: {rust} · Царап.: {scratch} ·
                    Вмятины: {dent}
                  </figcaption>
                </figure>
              )
            )}
          </div>
        </section>
      ) : null}
    </main>
  );
}
