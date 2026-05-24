import { Textarea, type TextareaProps } from "flowbite-react";

export type AppTextareaProps = TextareaProps;

export function AppTextarea(props: AppTextareaProps) {
  return <Textarea {...props} />;
}
