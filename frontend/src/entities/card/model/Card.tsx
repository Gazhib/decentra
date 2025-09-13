import { motion } from "framer-motion";
export default function Card({
  label,
  children,
}: {
  label: string;
  children: React.ReactNode;
}) {
  return (
    <motion.article
      initial={{ opacity: 0, y: 50 }}
      animate={{ opacity: 1, y: 0 }}
      transition={{ duration: 0.5 }}
      className="w-full flex flex-col gap-[10px] bg-gray-100/50 p-[20px] rounded-[16px] shadow"
    >
      <span className="text-[2.5rem] font-bold">{label}</span>
      <span className="text-[1.5rem] text-black/80">{children}</span>
    </motion.article>
  );
}
