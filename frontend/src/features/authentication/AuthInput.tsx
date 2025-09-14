import { motion } from "framer-motion";

type Props = {
  placeholder?: string;
  label?: string;
  name?: string;
  type?: string;
};

export default function AuthInput({ placeholder, label, name, type }: Props) {
  return (
    <motion.div
      initial={{ opacity: 0, y: 50 }}
      animate={{ opacity: 1, y: 0 }}
      transition={{ duration: 0.5 }}
      className="flex flex-col w-full gap-[5px]"
    >
      {label ? <label className="text-[14px]">{label}</label> : null}
      <input
        className="h-[40px] pl-[15px] text-[14px] border-[1px] rounded-[6px] border-[rgba(218,218,218,1)] w-full"
        placeholder={placeholder}
        name={name}
        type={type}
      />
    </motion.div>
  );
}
