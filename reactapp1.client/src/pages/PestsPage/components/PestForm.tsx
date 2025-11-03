import { useEffect, useMemo } from "react";
import { useForm } from "react-hook-form";
import { IPest } from "../../../interfaces/IPest";
import { formStyle } from "../../../styles/formStyle";

interface Props {
    storePest: (p: IPest) => void;
    pest?: IPest;
    deletePest: (id?: number) => void;
}

export function PestForm({ storePest, pest, deletePest }: Props) {
    const defaults = useMemo<IPest>(
        () => ({
            id: undefined,
            name: "",
            imageUrl: "",
        }),
        []
    );

    const { register, handleSubmit, reset } = useForm<IPest>({
        defaultValues: defaults,
    });

    useEffect(() => {
        reset(pest ?? defaults);
    }, [pest, reset, defaults]);

    const onSubmit = handleSubmit((data, e) => {
        const nativeEvent = e?.nativeEvent as SubmitEvent | undefined;
        const submitter = nativeEvent?.submitter as HTMLButtonElement | undefined;
        const action = submitter?.value;

        if (action === "delete") {
            if (data.id) {
                deletePest?.(data.id);
            }
            reset(defaults);
            return;
        }

        if (action === "new") {
            data.id = undefined;
        }

        storePest(data);

        if (action === "new") {
            reset(defaults);
        }
    });

    return (
        <form onSubmit={onSubmit} className="flex flex-col gap-3">
            <input type="hidden" {...register("id")} />

            <div>
                <label htmlFor="name" className={formStyle.label}>Pavadinimas</label>
                <input
                    id="name"
                    className={formStyle.input}
                    {...register("name", { required: true, maxLength: 100 })}
                />
            </div>

            <div>
                <label htmlFor="imageUrl" className={formStyle.label}>Nuotrauka</label>
                <input
                    id="imageUrl"
                    type="url"
                    className={formStyle.input}
                    {...register("imageUrl", { maxLength: 300 })}
                />
            </div>

            <div className="flex flex-col sm:flex-row gap-2">
                <button className={formStyle.button} type="submit" value="update">Atnaujinti</button>
                <button className={formStyle.button} type="submit" value="new">Naujas</button>
                <button className={formStyle.button} type="submit" value="delete">Ištrinti</button>
            </div>
        </form>
    );
}

export default PestForm;