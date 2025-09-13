interface ImageUploaderProps {
  label: string;
  addPhoto: (e: React.ChangeEvent<HTMLInputElement>, i: number) => void;
  index: number;
}

export default function ImageUploader({
  label,
  addPhoto,
  index,
}: ImageUploaderProps) {
  return (
    <label
      className="flex flex-col text-gray-500 items-center justify-center border-2 border-dashed 
    border-gray-300 rounded-[16px] cursor-pointer hover:border-[#a7e92f] hover:bg-[#8ec428] hover:text-white 
    transition p-[3rem] box-border w-80 h-50"
    >
      <span className="text-[1rem]">{label}</span>
      <input
        onChange={(e) => addPhoto(e, index)}
        type="file"
        accept="image/*"
        className="hidden"
      />
    </label>
  );
}
