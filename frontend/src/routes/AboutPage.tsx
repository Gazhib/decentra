import Card from "../entities/card/model/Card";
import { motion } from "framer-motion";
export default function AboutPage() {
  const cardInformation = [
    {
      label: "Наша миссия",
      description:
        "Добро пожаловать на нашу платформу! Мы стремимся предоставить вам лучший опыт в области управления транспортными средствами. Наша миссия - сделать процесс одобрения транспортных средств максимально простым и прозрачным при помощи искуственного интеллекта. А так же обеспечить клиентам максимальный комфорт и безопасность при использовании наших услуг.",
    },
    {
      label: "Наши услуги",
      description:
        "Мы предлагаем широкий спектр услуг, включая быструю проверку транспортных средств, автоматическое одобрение и интеграцию с различными платформами для удобства наших пользователей.",
    },
    {
      label: "Почему выбирают нас",
      description:
        "Наши клиенты ценят нас за надежность, скорость и качество обслуживания. Мы используем передовые технологии, чтобы обеспечить безопасность и удовлетворение потребностей наших пользователей.",
    },
  ];

  return (
    <main className="w-full h-full flex flex-row gap-[20px] px-[50px] py-[20px] overflow-x-hidden bg-gradient-to-b from-gray-100 to-[#a7e92f]">
      <section className="w-full flex flex-col gap-[10px]">
        {cardInformation.map(({ label, description }, index) => {
          return (
            <Card key={`${label}-${index}`} label={label}>
              {description}
            </Card>
          );
        })}
      </section>
      <section className="w-full flex flex-col justify-around items-center">
        <motion.img
          initial={{ opacity: 0, y: 50 }}
          animate={{ opacity: 1, y: 0 }}
          transition={{ duration: 0.5 }}
          className="w-[70%] h-auto object-cover rounded-[16px] shadow"
          src="https://d39raawggeifpx.cloudfront.net/styles/16_9_desktop/s3/articleimages/bneCompany_Russia_taxi_ecommerce_TMT_inDrive_car_Cropped.jpg"
        />
        <motion.img
          initial={{ opacity: 0, y: 50 }}
          animate={{ opacity: 1, y: 0 }}
          transition={{ duration: 0.5 }}
          className="w-[70%] h-auto object-cover rounded-[16px] shadow"
          src="https://www.africa-newsroom.com/files/thumb/6a2fdd06d0c4432/600/418"
        />
      </section>
    </main>
  );
}
