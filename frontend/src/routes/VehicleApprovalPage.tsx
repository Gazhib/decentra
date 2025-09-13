import { Form } from "react-router-dom";
import ImageUploader from "../entities/photoTaker/ui/ImageUploader";
import { useState } from "react";

type FileArray = {
  label: string;
  file: File | null;
};

export default function VehicleApprovalPage() {
  const [previews, setPreviews] = useState<string[]>(Array(4).fill(null));

  const [files, setFiles] = useState<FileArray[]>([
    {
      label: "Передняя часть",
      file: null,
    },
    {
      label: "Задняя часть",
      file: null,
    },
    {
      label: "Левая часть",
      file: null,
    },
    {
      label: "Правая часть",
      file: null,
    },
  ]);

  const addPhoto = (e: React.ChangeEvent<HTMLInputElement>, i: number) => {
    if (e.target.files && e.target.files[0]) {
      setFiles((prev) => {
        const newFiles = [...prev];
        newFiles[i].file = e.target.files![0];
        return newFiles;
      });

      setPreviews((prev) => {
        const newPreviews = [...prev];
        newPreviews[i] = URL.createObjectURL(e.target.files![0]);
        return newPreviews;
      });
    }
  };

  return (
    <main className="max-w-full min-h-full flex flex-col items-center justify-center gap-[20px] bg-white px-[50px] py-[20px] overflow-x-hidden">
      <section className="w-full flex flex-col items-center justify-center gap-[10px]">
        <span className="text-[1.5rem] font-bold">
          Пожалуйста, загрузите фотографии вашего транспортного средства для
          одобрения.
        </span>
        <span className="text-[1rem] text-center text-black/80">
          Убедитесь, что фотографии четкие и показывают все стороны вашего
        </span>
      </section>
      <section className="flex flex-col items-center justify-center h-full gap-[20px]">
        <Form className="grid grid-cols-1 sm:grid-cols-2 items-center gap-[10px]">
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
        </Form>
      </section>
      <section className="w-full flex items-center justify-center flex-col gap-[10px]">
        <span className="text-[3rem]">Пример фотографий</span>
        <img
          className="block w-[50%] rounded-[16px] shadow h-auto object-cover"
          src="https://avtoelektrik-info.ru/wp-content/uploads/2019/08/fe044b5s-1920-1024x769.jpg"
        />
      </section>
    </main>
  );
}
