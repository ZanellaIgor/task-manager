import {z} from 'zod'

const optionalText = z.preprocess((value) => {
    if (value === '' || value === null || value === undefined) {
        return undefined
    }

    return value
}, z.string().max(200).optional())

export const categorySchema = z.object({
    name: z.string().min(1, 'Nome obrigatório').max(60),
    description: optionalText,
})

export type CategoryFormData = z.infer<typeof categorySchema>

