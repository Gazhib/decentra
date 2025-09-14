import type { Photo } from "./Picture";
import Picture from "./Picture";

export default function PictureList({ photos }: { photos: Photo[] }) {
  return (
    <section className="w-full h-full flex flex-row items-center justify-center gap-[2rem]">
      {photos && photos.length > 0 ? (
        photos.map((photo: Photo, index: number) => {
          const { image, dust, rust, scratch, dent } = photo;
          return (
            <Picture
              image={image}
              dust={dust}
              rust={rust}
              scratch={scratch}
              dent={dent}
              key={index}
              index={index}
            />
          );
        })
      ) : (
        <span>Нет доступных фотографий</span>
      )}
    </section>
  );
}
