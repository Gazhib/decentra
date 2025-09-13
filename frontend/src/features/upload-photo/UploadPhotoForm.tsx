import { Form } from "react-router-dom";
import ImageUploader from "../../entities/photoTaker/ui/ImageUploader";

export default function UploadPhotoForm({
  files,
  previews,
  addPhoto,
}: {
  files: { label: string; file: File | null }[];
  previews: string[];
  addPhoto: (e: React.ChangeEvent<HTMLInputElement>, i: number) => void;
}) {
  return (
    <Form method="post" className="flex flex-col items-center justify-center gap-[20px]">
      <div className="grid grid-cols-1 sm:grid-cols-2 items-center gap-[10px]">
        {files.map(({ label, file }, i) =>
          file === null ? (
            <ImageUploader
              key={label}
              label={label}
              addPhoto={addPhoto}
              index={i}
            />
          ) : (
            <img
              className="w-80 h-50 flex flex-col rounded-[16px] cursor-pointer object-cover"
              src={previews[i]}
            />
          )
        )}
      </div>
      <div className="w-full flex items-center justify-center mt-[20px] ">
        <button
          className="bg-[#a7e92f] py-[1rem] px-[3rem] rounded-[8px] cursor-pointer hover:bg-[#8ec428] hover:text-white transition duration-300"
          type="submit"
        >
          Получить результаты
        </button>
      </div>
    </Form>
  );
}
