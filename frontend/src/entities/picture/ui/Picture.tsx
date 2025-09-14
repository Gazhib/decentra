export type Photo = {
  image: string;
  dust: number;
  rust: number;
  scratch: number;
  dent: number;
};

export default function Picture({
  image,
  dust,
  rust,
  scratch,
  dent,
  index,
}: Photo & { index: number }) {
  return (
    <div key={image} className="flex flex-col items-center gap-[0.5rem]">
      <img
        src={`data:image/jpeg;base64,${image}`}
        alt={`Осмотр ${index + 1}`}
        className="w-[200px] h-[200px] object-cover border-2 border-gray-300 rounded-[8px]"
      />
      <div className="text-sm">
        <p>Пыль: {dust}</p>
        <p>Ржавчина: {rust}</p>
        <p>Царапины: {scratch}</p>
        <p>Вмятины: {dent}</p>
      </div>
    </div>
  );
}
