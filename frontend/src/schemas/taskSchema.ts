import {z} from 'zod'

const optionalText = z.preprocess((value) => {
    if (value === '' || value === null || value === undefined) {
        return undefined
    }

    return value
}, z.string().max(500).optional())

const optionalDateText = z.preprocess((value) => {
    if (value === '' || value === null || value === undefined) {
        return undefined
    }

    return value
}, z.string().optional())

export const taskSchema = z.object({
    title: z.string().min(1, 'Título obrigatório').max(100, 'Máximo 100 caracteres'),
    description: optionalText,
    categoryId: z.preprocess((value) => {
        if (value === '' || value === null || value === undefined) {
            return undefined
        }

        const parsed = typeof value === 'number' ? value : Number(value)
        return Number.isNaN(parsed) ? value : parsed
    }, z.number({error: 'Categoria obrigatória'}).int().positive('Categoria obrigatória')),
    priority: z.enum(['Low', 'Medium', 'High']),
    dueDate: optionalDateText,
})

export type TaskFormData = z.infer<typeof taskSchema>
